using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Globalization;
using Yumiko.Commands;
using Yumiko.Services;
using Yumiko.Utils;

namespace Yumiko
{
    public class Program
    {
        internal static DiscordClient DiscordClient { get; set; } = null!;
        internal static SlashCommandsExtension ApplicationCommands { get; set; } = null!;
        internal static ServiceProvider ServiceProvider { get; set; } = null!;
        internal static IConfigurationRoot Configuration { get; private set; } = null!;
        public static bool Debug { get; private set; }
        public static bool TopggEnabled { get; private set; }
        public static DiscordChannel LogChannelApplicationCommands { get; private set; } = null!;
        public static DiscordChannel LogChannelGuilds { get; private set; } = null!;
        public static DiscordChannel LogChannelErrors { get; private set; } = null!;

        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            DebugMode();

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile(Path.Join("res", "config.json"), true, true);
            configurationBuilder.AddCommandLine(args);
            Configuration = configurationBuilder.Build();
            services.AddSingleton(Configuration);

            services.AddLogging(loggingBuilder =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Is(Debug ? LogEventLevel.Debug : LogEventLevel.Information)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File($"logs/{DateTime.Now.ToString("dd'-'MM'-'yyyy' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}");

                Log.Logger = loggerConfiguration.CreateLogger().ForContext<Program>();

                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger, dispose: true);
            });

            ServiceProvider = services.BuildServiceProvider();

            if (!ConfigFilesCheck())
            {
                return;
            }

            DiscordClient = new DiscordClient(new()
            {
                Token = Configuration.GetValue<string>(Debug ? "tokens:discord:testing" : "tokens:discord:production"),
                Intents = DiscordIntents.Guilds,
                LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>(),
                ReconnectIndefinitely = true
            });

            DiscordClient.UseInteractivity(new InteractivityConfiguration()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.DeleteMessage,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromSeconds(Configuration.GetValue<double>("timeouts:general", 60))
            });

            DiscordClient.Ready += Client_Ready;
            DiscordClient.Resumed += Client_Resumed;
            DiscordClient.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            DiscordClient.GuildCreated += Client_GuildCreated;
            DiscordClient.GuildDeleted += Client_GuildDeleted;
            DiscordClient.ClientErrored += Client_ClientError;
            DiscordClient.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            DiscordClient.ModalSubmitted += Client_ModalSubmitted;

            ApplicationCommands = DiscordClient.UseSlashCommands(new()
            {
                Services = ServiceProvider
            });

            ApplicationCommands.SlashCommandExecuted += SlashCommands_SlashCommandExecuted;
            ApplicationCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;

            var logGuildId = Configuration.GetValue<ulong>("loggin:guild_id");

            if (Debug)
            {
                ApplicationCommands.RegisterCommands<Games>(logGuildId);
                ApplicationCommands.RegisterCommands<Interact>(logGuildId);
                ApplicationCommands.RegisterCommands<Anilist>(logGuildId);
                ApplicationCommands.RegisterCommands<Other>(logGuildId);
                ApplicationCommands.RegisterCommands<Help>(logGuildId);
            }
            else
            {
                ApplicationCommands.RegisterCommands<Games>();
                ApplicationCommands.RegisterCommands<Interact>();
                ApplicationCommands.RegisterCommands<Anilist>();
                ApplicationCommands.RegisterCommands<Other>();
                ApplicationCommands.RegisterCommands<Help>();
            }

            ApplicationCommands.RegisterCommands<Owner>(logGuildId);

            await DiscordClient.ConnectAsync(new DiscordActivity { ActivityType = ActivityType.ListeningTo, Name = "/help" }, UserStatus.Online);

            await Task.Delay(-1);
        }

        internal static bool ConfigFilesCheck()
        {
            var configPath = Path.Join("res", "config.json");
            if (!File.Exists(configPath))
            {
                Log.Fatal($"{configPath} was not found!");
                return false;
            }

            var firebasePath = Path.Join("res", "firebase.json");
            if (!File.Exists(firebasePath))
            {
                Log.Fatal($"{firebasePath} was not found!");
                return false;
            }

            TopggEnabled = Configuration.GetValue<bool>("topgg_enabled");

            return true;
        }

        internal static void DebugMode()
        {
#if DEBUG
            Debug = true;
#else
            Debug = false;
#endif
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("DiscordClient ready to fire events");
            return Task.CompletedTask;
        }

        private static Task Client_Resumed(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("DiscordClient resumed");
            return Task.CompletedTask;
        }

        private static Task Client_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                string env = Debug ? "testing" : "production";
                try
                {
                    var logGuild = await DiscordClient.GetGuildAsync(Configuration.GetValue<ulong>("loggin:guild_id"));

                    LogChannelApplicationCommands = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:application_commands"));
                    LogChannelGuilds = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:guilds"));
                    LogChannelErrors = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:errors"));

                    sender.Logger.LogInformation("Log guild and channels initialized", DateTime.Now);
                }
                catch (Exception)
                {
                    sender.Logger.LogCritical("Could not get loggin guild and channels");
                    await DiscordClient.DisconnectAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await LogChannelGuilds.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Author = new()
                    {
                        IconUrl = e.Guild.IconUrl,
                        Name = $"{e.Guild.Name}",
                    },
                    Title = "New Guild",
                    Description =
                    $"   **Id**: {e.Guild.Id}\n" +
                    $"   **Members**: {e.Guild.MemberCount - 1}\n" +
                    $"   **Owner**: {e.Guild.Owner.Username}#{e.Guild.Owner.Discriminator}\n\n" +
                    $"   **Guild count**: {sender.Guilds.Count}",
                    Footer = new()
                    {
                        Text = $"{DateTimeOffset.Now}",
                    },
                    Color = DiscordColor.Green,
                });
                if (TopggEnabled && !Debug)
                {
                    await Common.UpdateStatsTopGGAsync(sender, Configuration);
                }
            });
            return Task.CompletedTask;
        }

        private static Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await LogChannelGuilds.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Author = new()
                    {
                        IconUrl = e.Guild.IconUrl,
                        Name = $"{e.Guild.Name}",
                    },
                    Title = "Bye-bye guild",
                    Description =
                    $"   **Id**: {e.Guild.Id}\n" +
                    $"   **Members**: {e.Guild.MemberCount - 1}\n" +
                    $"   **Guild count**: {sender.Guilds.Count}",
                    Footer = new()
                    {
                        Text = $"{DateTimeOffset.Now}",
                    },
                    Color = DiscordColor.Red,
                });
                if (TopggEnabled && !Debug)
                {
                    await Common.UpdateStatsTopGGAsync(sender, Configuration);
                }
            });
            return Task.CompletedTask;
        }

        private static Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError("Client error. Check errors channel log");
            _ = Task.Run(async () =>
            {
                await LogChannelErrors.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Title = "Client Error",
                    Description = $"{Formatter.BlockCode(e.Exception.StackTrace)}",
                    Footer = new()
                    {
                        Text = $"{DateTimeOffset.Now}",
                    },
                    Color = DiscordColor.Red,
                }.AddField("Type", $"{e.Exception.GetType()}", false)
                .AddField("Message", $"{e.Exception.Message}", false)
                .AddField("Event", $"{e.EventName}", false));
            });
            return Task.CompletedTask;
        }

        private static Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id.StartsWith("quiz-modal-"))
            {
                _ = Task.Run(async () =>
                {
                    var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Guild.Id, e.Channel.Id);
                    if (trivia != null)
                    {
                        var btnInteraction = e.Interaction;
                        string modalId = $"quiz-modal-{btnInteraction.Id}";

                        var modal = new DiscordInteractionResponseBuilder()
                            .WithCustomId(modalId)
                            .WithTitle($"Guess the {trivia.Title}")
                            .AddComponents(new TextInputComponent(label: trivia.Title?.UppercaseFirst(), customId: "guess"));

                        await btnInteraction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                    }
                });
            }
            else if (e.Id.StartsWith("quiz-cancel-"))
            {
                _ = Task.Run(async () =>
                {
                    var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Guild.Id, e.Channel.Id);
                    if (trivia != null)
                    {
                        if (trivia.CreatedBy?.Id == e.User.Id)
                        {
                            trivia.Canceled = true;

                            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                .AsEphemeral(true)
                                .AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"You have canceled the game!",
                                    Color = DiscordColor.Red,
                                }));
                        }
                    }
                });
            }
            else
            {
                if (!e.Id.StartsWith("modal-"))
                {
                    _ = Task.Run(async () =>
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    });
                }
            }

            return Task.CompletedTask;
        }

        private static Task Client_ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            if (e.Interaction.Data.CustomId.StartsWith("quiz-modal-"))
            {
                _ = Task.Run(async () =>
                {
                    var modalInteraction = e.Interaction;
                    var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Interaction.Guild.Id, e.Interaction.Channel.Id);
                    if (trivia != null)
                    {
                        var value = e.Values["guess"];
                        if (trivia.CurrentRound.Matches.Contains(value.ToLower()))
                        {
                            await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .AsEphemeral(true)
                                .AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = "You guessed it",
                                    Color = DiscordColor.Green,
                                }));

                            trivia.CurrentRound.Guessed = true;
                            trivia.CurrentRound.Guesser = e.Interaction.User;
                            trivia.CurrentRound.GuessTime = modalInteraction.CreationTimestamp;
                        }
                        else
                        {
                            await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .AsEphemeral(true)
                                .AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = "Wrong choice",
                                    Description = $"Your attempt: `{value}`",
                                    Color = DiscordColor.Red,
                                }));
                        }
                    }
                    else
                    {
                        await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .AsEphemeral(true)
                                .AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = "No trivia",
                                    Description = $"There is no trivia in this channel",
                                    Color = DiscordColor.Red,
                                }));
                    }
                });
            }

            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            e.Handled = true;
            _ = Task.Run(async () =>
            {
                await LogChannelApplicationCommands.SendMessageAsync(Common.LogInteractionCommand(e, "Slash Command executed", true, false));
            });
            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            e.Handled = true;
            _ = Task.Run(async () =>
            {
                if (e.Exception is SlashExecutionChecksFailedException ex)
                {
                    await e.Context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    foreach (SlashCheckBaseAttribute check in ex.FailedChecks)
                    {
                        switch (check)
                        {
                            case SlashRequireOwnerAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"Access denied",
                                    Description = $"Only the bot owner can execute this command",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireBotPermissionsAttribute bp:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"Bot permission required",
                                    Description = $"{e.Context.Client.CurrentUser.Username} need the {Formatter.InlineCode($"{bp.Permissions}")} to execute this command",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireUserPermissionsAttribute up:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"User Permission required",
                                    Description = $"You need the {Formatter.InlineCode($"{up.Permissions}")} permission to execute this command",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequirePermissionsAttribute ubp:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"Permission required",
                                    Description = $"You and {e.Context.Client.CurrentUser.Username} needs the {Formatter.InlineCode($"{ubp.Permissions}")} permission to execute this command",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireGuildAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"Guild required",
                                    Description = $"You can only execute this command in a guild",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireDirectMessageAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = $"DM required",
                                    Description = $"You can only execute this command in direct messages",
                                    Color = DiscordColor.Red,
                                }));
                                break;
                        }
                    }
                }
                else
                {
                    await LogChannelErrors.SendMessageAsync(Common.LogInteractionCommand(e, "Unknown error (Application Commands)", true, true));

                    // If the error is from a trivia game // TODO: check this
                    Singleton.GetInstance().RemoveCurrentTrivia(e.Context.Guild.Id, e.Context.Channel.Id);
                }
            });
            return Task.CompletedTask;
        }
    }
}
