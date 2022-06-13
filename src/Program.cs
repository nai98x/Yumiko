global using Yumiko.Commands;
global using Yumiko.Datatypes;
global using Yumiko.Datatypes.Firebase;
global using Yumiko.Enums;
global using Yumiko.Providers;
global using Yumiko.Services;
global using Yumiko.Services.Firebase;
global using Yumiko.Utils;
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
using System.Diagnostics;
using System.Globalization;

namespace Yumiko
{
    public class Program
    {
        internal static DiscordShardedClient DiscordShardedClient { get; set; } = null!;
        internal static ServiceProvider ServiceProvider { get; set; } = null!;
        internal static IConfigurationRoot Configuration { get; private set; } = null!;
        public static bool Debug { get; private set; }
        public static bool TopggEnabled { get; private set; }
        public static DiscordChannel LogChannelApplicationCommands { get; private set; } = null!;
        public static DiscordChannel LogChannelGuilds { get; private set; } = null!;
        public static DiscordChannel LogChannelErrors { get; private set; } = null!;
        public static Stopwatch Stopwatch { get; private set; } = null!;

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
                    .Filter.ByExcluding("StartsWith(Message, 'Unknown')")
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

            DiscordShardedClient = new DiscordShardedClient(new()
            {
                Token = Configuration.GetValue<string>(Debug ? "tokens:discord:testing" : "tokens:discord:production"),
                Intents = DiscordIntents.Guilds,
                LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>(),
                ReconnectIndefinitely = true
            });

            await DiscordShardedClient.UseInteractivityAsync(new InteractivityConfiguration()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.DeleteMessage,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromSeconds(Configuration.GetValue<double>("timeouts:general", 60))
            });

            DiscordShardedClient.Ready += Client_Ready;
            DiscordShardedClient.Resumed += Client_Resumed;
            DiscordShardedClient.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            DiscordShardedClient.GuildCreated += Client_GuildCreated;
            DiscordShardedClient.GuildDeleted += Client_GuildDeleted;
            DiscordShardedClient.ClientErrored += Client_ClientError;
            DiscordShardedClient.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            DiscordShardedClient.ModalSubmitted += Client_ModalSubmitted;

            await DiscordShardedClient.StartAsync();

            ulong logGuildId = Configuration.GetValue<ulong>("loggin:guild_id");
            int shardCount = DiscordShardedClient.ShardClients.Count;
            int logGuildShard = ((int)logGuildId >> 22) % shardCount;

            var config = new SlashCommandsConfiguration()
            {
                Services = ServiceProvider
            };

            foreach (var keyValuePair in (await DiscordShardedClient.UseSlashCommandsAsync(config)))
            {
                var slashShardExtension = keyValuePair.Value;

                slashShardExtension.SlashCommandExecuted += SlashCommands_SlashCommandExecuted;
                slashShardExtension.SlashCommandErrored += SlashCommands_SlashCommandErrored;

                if (Debug && keyValuePair.Key == logGuildShard)
                {
                    slashShardExtension.RegisterCommands<Games>(logGuildId);
                    slashShardExtension.RegisterCommands<Stats>(logGuildId);
                    slashShardExtension.RegisterCommands<Interact>(logGuildId);
                    slashShardExtension.RegisterCommands<Anilist>(logGuildId);
                    slashShardExtension.RegisterCommands<Misc>(logGuildId);
                    slashShardExtension.RegisterCommands<Help>(logGuildId);
                }
                else
                {
                    slashShardExtension.RegisterCommands<Games>();
                    slashShardExtension.RegisterCommands<Stats>();
                    slashShardExtension.RegisterCommands<Interact>();
                    slashShardExtension.RegisterCommands<Anilist>();
                    slashShardExtension.RegisterCommands<Misc>();
                    slashShardExtension.RegisterCommands<Help>();
                }

                if (keyValuePair.Key == logGuildShard)
                {
                    slashShardExtension.RegisterCommands<Owner>(logGuildId);
                }
            }

            Stopwatch = Stopwatch.StartNew();

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
            _ = Task.Run(async () =>
            {
                await DiscordShardedClient.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.ListeningTo, Name = "/help" }, UserStatus.Online);
            });
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

                var logGuildId = Configuration.GetValue<ulong>("loggin:guild_id");
                var client = DiscordShardedClient.GetShard(logGuildId);

                if (client != null)
                {
                    var logGuild = await client.GetGuildAsync(logGuildId);

                    LogChannelApplicationCommands = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:application_commands"));
                    LogChannelGuilds = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:guilds"));
                    LogChannelErrors = logGuild.GetChannel(Configuration.GetValue<ulong>($"loggin:{env}:errors"));

                    sender.Logger.LogInformation("Log guild and channels initialized", DateTime.Now);
                }
                else
                {
                    sender.Logger.LogCritical("Could not get loggin guild and channels");
                    await DiscordShardedClient.StopAsync();
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
                await LogChannelApplicationCommands.SendMessageAsync(LogUtils.LogSlashCommand(e));
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
                    await LogChannelErrors.SendMessageAsync(LogUtils.LogSlashCommandError(e));

                    // If the error is from a trivia game // TODO: check this
                    Singleton.GetInstance().RemoveCurrentTrivia(e.Context.Guild.Id, e.Context.Channel.Id);
                }
            });
            return Task.CompletedTask;
        }
    }
}
