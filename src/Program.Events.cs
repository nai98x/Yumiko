namespace Yumiko
{
    using Microsoft.Extensions.Logging;

    public partial class Program
    {
        private static Task Client_Ready(DiscordClient sender, SessionReadyEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await DiscordShardedClient.UpdateStatusAsync(new DiscordActivity { ActivityType = ActivityType.ListeningTo, Name = "/help" }, UserStatus.Online);
            });
            sender.Logger.LogInformation("DiscordShardedClient session ready to fire events");
            return Task.CompletedTask;
        }

        private static Task Client_Resumed(DiscordClient sender, SessionReadyEventArgs e)
        {
            sender.Logger.LogInformation("DiscordShardedClient session resumed");
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
                    var logGuild = client.Guilds[logGuildId];

                    LogChannelGuilds = logGuild.Channels[ConfigurationUtils.GetConfiguration<ulong>(Configuration, Debug ? Configurations.LogginTestingGuilds : Configurations.LogginProductionGuilds)];
                    LogChannelErrors = logGuild.Channels[ConfigurationUtils.GetConfiguration<ulong>(Configuration, Debug ? Configurations.LogginTestingErrors : Configurations.LogginProductionErrors)];

                    sender.Logger.LogInformation("Log guild and channels initialized", DateTime.Now);

                    await Singleton.GetInstance().UpdateCachedMediaAsync();
                    sender.Logger.LogInformation("Media cache from AniList initialized", DateTime.Now);
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
                    $"   **Members**: {e.Guild.MemberCount}\n" +
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
                    await Common.UpdateStatsTopGGAsync(sender.CurrentApplication.Id, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
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
                    $"   **Members**: {e.Guild.MemberCount}\n" +
                    $"   **Guild count**: {sender.Guilds.Count}",
                    Footer = new()
                    {
                        Text = $"{DateTimeOffset.Now}",
                    },
                    Color = DiscordColor.Red,
                });
                if (TopggEnabled && !Debug)
                {
                    await Common.UpdateStatsTopGGAsync(sender.CurrentApplication.Id, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
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

            if (e.Id.StartsWith("poll-select-"))
            {
                _ = PollService.HandleInteraction(e);
            }
            else if (e.Id.StartsWith("quiz-cancel-"))
            {
                _ = TriviaService.HandleTriviaCancelledInteraction(e);
            }
            else if (e.Id.StartsWith("quiz-round-"))
            {
                _ = TriviaService.HandleTriviaRoundInteraction(e);
            }
            else if (!e.Id.StartsWith("modal-") && !e.Id.StartsWith("quiz-modal-") && !e.Id.StartsWith("quiz-cancel-"))
            {
                _ = Task.Run(async () =>
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                });
            }

            return Task.CompletedTask;
        }

        private static Task Client_ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            sender.Client.Logger.LogInformation("Slash command executed: {args}", LogUtils.GetSlashCommandArgs(e, true));
            Singleton.GetInstance().UpdateCommandUsed(LogUtils.GetSlashCommandArgs(e, false));
            return Task.CompletedTask;
        }

        private static Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
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
        private static Task SlashCommands_ContextMenuExecuted(SlashCommandsExtension sender, ContextMenuExecutedEventArgs e)
        {
            sender.Client.Logger.LogInformation("Context menu executed: {args}", LogUtils.GetContextMenu(e));
            Singleton.GetInstance().UpdateCommandUsed(LogUtils.GetContextMenu(e));
            return Task.CompletedTask;
        }

        private static Task SlashCommands_ContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await LogChannelErrors.SendMessageAsync(LogUtils.LogContextMenuError(e));
            });
            return Task.CompletedTask;
        }
    }
}
