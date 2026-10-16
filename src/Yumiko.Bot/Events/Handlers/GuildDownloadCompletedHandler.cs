using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.State;
using Yumiko.Infrastructure.Database;

namespace Yumiko.Bot.Events.Handlers;

public sealed class GuildDownloadCompletedHandler(
    DiscordBotService discordBotService,
    AnilistMediaCacheState mediaCache,
    MediaCacheRefresher mediaCacheRefresher,
    DbConnectionFactory connectionFactory,
    ILogger<GuildDownloadCompletedHandler> logger)
{
    public async Task Handle(DiscordClient client, GuildDownloadCompletedEventArgs args)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            discordBotService.SetChannels();
            logger.LogInformation("Guild and log channels initialized");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Could not resolve the guild or the log channels; aborting initialization");
            discordBotService.SetInitializationFailed();
            return;
        }

        // Games, stats and the AniList link all go through the database: without it the bot answers
        // nothing useful, so it stays marked as not ready instead of failing command by command.
        try
        {
            await connectionFactory.EnsureConnectionAsync();
            logger.LogInformation("Database connection established");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Could not connect to the database; aborting initialization");
            discordBotService.SetInitializationFailed();
            await SendToConfigBotsAsync(new DiscordMessageBuilder().WithContent(
                "⚠️ **Could not connect to the database.** The bot stays marked as not ready and its commands will not run. " +
                "Check the connection string and that the database server is reachable."));
            return;
        }

        bool mediaFullyLoaded = await mediaCacheRefresher.WarmUpAsync();

        discordBotService.SetInitialized();
        sw.Stop();

        logger.LogInformation(
            "Bot initialized in {Seconds:0.00}s | Guilds: {Guilds} | Cached media: {Media}",
            sw.Elapsed.TotalSeconds,
            client.Guilds.Count,
            mediaCache.Media.Count);

        List<DiscordComponent> components =
        [
            new DiscordTextDisplayComponent("# ✅ Bot initialized successfully"),
            new DiscordSeparatorComponent(divider: true, spacing: DiscordSeparatorSpacing.Large),
            new DiscordTextDisplayComponent(
                $"⏱️ **Initialization time:** {sw.Elapsed.TotalSeconds:0.00} s\n" +
                $"🌐 **Guilds:** {client.Guilds.Count}\n" +
                $"🎬 **Cached media:** {mediaCache.Media.Count}" +
                (mediaFullyLoaded ? string.Empty : " (the rest keeps loading in background)")),
        ];

        if (mediaCache.IsEmpty)
        {
            components.Add(new DiscordSeparatorComponent(divider: true, spacing: DiscordSeparatorSpacing.Small));
            components.Add(new DiscordTextDisplayComponent(
                "⚠️ **Empty media cache:** Higher or Lower will not be available until the next successful refresh."));
        }

        if (discordBotService.Debug)
        {
            components.Add(new DiscordSeparatorComponent(divider: true, spacing: DiscordSeparatorSpacing.Small));
            components.Add(new DiscordTextDisplayComponent("-# 🐛 Debug mode enabled"));
        }

        await SendToConfigBotsAsync(new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(new DiscordContainerComponent(components, color: YumikoColors.Primary)));
    }

    private async Task SendToConfigBotsAsync(DiscordMessageBuilder builder)
    {
        try
        {
            await discordBotService.LogChannelConfigBots.SendMessageAsync(builder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send the initialization message to the ConfigBots channel");
        }
    }
}
