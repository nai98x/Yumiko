using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Services;

/// <summary>
/// Fills the media cache used by Higher or Lower. It is called by the startup
/// (<c>GuildDownloadCompleted</c>) and by the daily task.
/// </summary>
public sealed class MediaCacheRefresher(
    AnilistMediaCacheState mediaCache,
    IAnilistClient anilistClient,
    GamesSettings gamesSettings,
    ILogger<MediaCacheRefresher> logger)
{
    /// <summary>
    /// Startup load: the cache is published as soon as it holds the warm-up amount and keeps growing
    /// page by page while the crawl continues in background. Completes with <c>true</c> when the whole
    /// crawl already ended (or failed), <c>false</c> when it is still running.
    /// </summary>
    public Task<bool> WarmUpAsync()
    {
        TaskCompletionSource<bool> warmedUp = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(() => CrawlAsync(warmedUp, CancellationToken.None));
        return warmedUp.Task;
    }

    /// <summary>
    /// The swap is atomic: until the crawl finishes, the games keep seeing the previous
    /// cache. If the query fails nothing is touched, so an AniList outage does not empty the pool.
    /// </summary>
    public Task RefreshAsync(CancellationToken cancellationToken = default) => CrawlAsync(null, cancellationToken);

    private async Task CrawlAsync(TaskCompletionSource<bool>? warmedUp, CancellationToken cancellationToken)
    {
        try
        {
            GameMediaQuery query = new()
            {
                Type = MediaType.ANIME,
                IncludeAdult = false,
                ExcludeUnreleased = true,
            };

            List<Anime> media = [];

            for (int pageNumber = gamesSettings.MediaCachePageFrom; pageNumber <= gamesSettings.MediaCachePageTo; pageNumber++)
            {
                GameMediaPage page = await anilistClient.GetGameMediaPageAsync(query, pageNumber, cancellationToken);
                media.AddRange(page.Media);

                if (!page.HasNextPage)
                {
                    break;
                }

                if (warmedUp is not null && media.Count >= gamesSettings.MediaCacheWarmUpEntries)
                {
                    mediaCache.Replace([.. media]);
                    warmedUp.TrySetResult(false);
                }

                if (warmedUp is null || warmedUp.Task.IsCompleted)
                {
                    await Task.Delay(gamesSettings.MediaCachePageDelay, cancellationToken);
                }
            }

            // AniList sometimes answers HTTP 200 with an empty page and no errors while it is degraded.
            if (media.Count == 0)
            {
                logger.LogWarning("AniList returned no media for the cache refresh; the previous one is kept");
                return;
            }

            mediaCache.Replace(media);
            logger.LogInformation("AniList media cache refreshed: {Count} entries", media.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not refresh the AniList media cache; the previous one is kept");
        }
        finally
        {
            warmedUp?.TrySetResult(true);
        }
    }
}
