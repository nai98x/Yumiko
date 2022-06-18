namespace Yumiko
{
    using Microsoft.Extensions.Logging;

    public partial class Program
    {
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
                var logGuildId = ConfigurationUtils.GetConfiguration<ulong>(Configuration, Configurations.LogginGuildId);
                var client = DiscordShardedClient.GetShard(logGuildId);

                if (client != null)
                {
                    var logGuild = await client.GetGuildAsync(logGuildId);

                    LogChannelGuilds = logGuild.GetChannel(Debug ? ConfigurationUtils.GetConfiguration<ulong>(Configuration, Configurations.LogginTestingGuilds) : ConfigurationUtils.GetConfiguration<ulong>(Configuration, Configurations.LogginProductionGuilds));
                    LogChannelErrors = logGuild.GetChannel(Debug ? ConfigurationUtils.GetConfiguration<ulong>(Configuration, Configurations.LogginTestingErrors) : ConfigurationUtils.GetConfiguration<ulong>(Configuration, Configurations.LogginProductionErrors));

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
            sender.Logger.LogInformation("Guild added: {Name} | Guild count: {Count}", e.Guild.Name, sender.Guilds.Count);
            _ = Task.Run(async () =>
            {
                await LogChannelGuilds.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Author = new()
                    {
                        IconUrl = e.Guild.IconUrl,
                        Name = $"{e.Guild.Name}",
                    },
                    Title = "Guild added",
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
                    await Common.UpdateStatsTopGGAsync(sender.CurrentApplication.Id, ConfigurationUtils.GetConfiguration<string>(Configuration, Enums.Configurations.TokenTopgg));
                }
            });
            return Task.CompletedTask;
        }

        private static Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            sender.Logger.LogInformation("Guild removed: {Name} | Guild count: {Count}", e.Guild.Name, sender.Guilds.Count);
            _ = Task.Run(async () =>
            {
                await LogChannelGuilds.SendMessageAsync(embed: new DiscordEmbedBuilder()
                {
                    Author = new()
                    {
                        IconUrl = e.Guild.IconUrl,
                        Name = $"{e.Guild.Name}",
                    },
                    Title = "Guild removed",
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
                    await Common.UpdateStatsTopGGAsync(sender.CurrentApplication.Id, ConfigurationUtils.GetConfiguration<string>(Configuration, Enums.Configurations.TokenTopgg));
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
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(e.Interaction.Locale!);
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
                            .WithTitle($"{translations.guess_the} {trivia.Title}")
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
                                    Title = translations.you_have_cancelled_the_game,
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
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(e.Interaction.Locale!);
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
                                    Title = translations.you_guessed,
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
                                    Title = translations.wrong_choice,
                                    Description = $"{translations.your_attempt}: `{value}`",
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
                                    Title = translations.error,
                                    Description = translations.no_current_trivia,
                                    Color = DiscordColor.Red,
                                }));
                    }
                });
            }

            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            string args = LogUtils.GetSlashCommandArgs(e);
            sender.Client.Logger.LogInformation("Slash command executed: {args}", args);
            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            e.Handled = true;
            _ = Task.Run(async () =>
            {
                if (e.Exception is SlashExecutionChecksFailedException ex)
                {
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(e.Context.Interaction.Locale!);
                    await e.Context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    foreach (SlashCheckBaseAttribute check in ex.FailedChecks)
                    {
                        switch (check)
                        {
                            case SlashRequireOwnerAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.access_denied,
                                    Description = translations.only_bot_owner,
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireBotPermissionsAttribute bp:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.bot_permission_required,
                                    Description = string.Format(translations.bot_permission_required_desc, e.Context.Client.CurrentUser.Username, bp.Permissions),
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireUserPermissionsAttribute up:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.user_permission_required,
                                    Description = string.Format(translations.user_permission_required_desc, up.Permissions),
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequirePermissionsAttribute ubp:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.permission_required,
                                    Description = string.Format(translations.permission_required_desc, e.Context.Client.CurrentUser.Username, ubp.Permissions),
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireGuildAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.guild_required,
                                    Description = translations.guild_required_desc,
                                    Color = DiscordColor.Red,
                                }));
                                break;
                            case SlashRequireDirectMessageAttribute:
                                await e.Context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Title = translations.dm_required,
                                    Description = translations.dm_required_desc,
                                    Color = DiscordColor.Red,
                                }));
                                break;
                        }
                    }
                }
                else
                {
                    await LogChannelErrors.SendMessageAsync(LogUtils.LogSlashCommandError(e));

                    if (e.Exception.StackTrace != null && e.Exception.StackTrace.Contains("Trivia"))
                    {
                        Singleton.GetInstance().RemoveCurrentTrivia(e.Context.Guild.Id, e.Context.Channel.Id);
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}
