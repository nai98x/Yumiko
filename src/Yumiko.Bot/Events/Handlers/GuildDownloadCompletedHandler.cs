using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
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
            return;
        }

        await mediaCacheRefresher.RefreshAsync();

        discordBotService.SetInitialized();
        sw.Stop();

        logger.LogInformation(
            "Bot initialized in {Seconds:0.00}s | Guilds: {Guilds} | Cached media: {Media}",
            sw.Elapsed.TotalSeconds,
            client.Guilds.Count,
            mediaCache.Media.Count);
    }
}
