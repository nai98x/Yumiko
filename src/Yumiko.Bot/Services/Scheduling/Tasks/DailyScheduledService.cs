using Microsoft.Extensions.Logging;

namespace Yumiko.Bot.Services.Scheduling.Tasks;

public sealed class DailyScheduledService(
    DiscordBotService discordBotService,
    MediaCacheRefresher mediaCacheRefresher,
    ILogger<DailyScheduledService> logger)
    : CronBackgroundService(discordBotService, logger)
{
    /// <summary>4 AM UTC: la franja más tranquila para el crawl de AniList.</summary>
    protected override string CronExpression => "0 4 * * *";

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        if (!Initialized)
        {
            return;
        }

        await mediaCacheRefresher.RefreshAsync(cancellationToken);
    }
}
