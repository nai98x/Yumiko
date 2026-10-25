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
    /// page by page while the crawl continues in background.
    /// </summary>
    public MediaCacheWarmUp WarmUp()
    {
        TaskCompletionSource<bool> warmedUp = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<bool> crawl = Task.Run(() => CrawlAsync(warmedUp, CancellationToken.None));
        return new MediaCacheWarmUp(warmedUp.Task, crawl);
    }

    /// <summary>
    /// The swap is atomic: until the crawl finishes, the games keep seeing the previous
    /// cache. If the query fails nothing is touched, so an AniList outage does not empty the pool.
    /// </summary>
    public Task RefreshAsync(CancellationToken cancellationToken = default) => CrawlAsync(null, cancellationToken);

    private async Task<bool> CrawlAsync(TaskCompletionSource<bool>? warmedUp, CancellationToken cancellationToken)
    {
        try
        {
            GameMediaQuery query = new()
            {
                Type = MediaType.ANIME,
                IncludeAdult = false,
                ExcludeUnreleased = true,
                IncludeCharacters = true,
                IncludeStudios = true,
            };

            List<IReadOnlyList<Anime>> pages = [];
            int count = 0;

            for (int pageNumber = gamesSettings.MediaCachePageFrom; pageNumber <= gamesSettings.MediaCachePageTo; pageNumber++)
            {
                GameMediaPage page = await anilistClient.GetGameMediaPageAsync(query, pageNumber, cancellationToken);
                pages.Add(page.Media);
                count += page.Media.Count;

                if (!page.HasNextPage)
                {
                    break;
                }

                if (warmedUp is not null && count >= gamesSettings.MediaCacheWarmUpEntries)
                {
                    mediaCache.Replace(gamesSettings.MediaCachePageFrom, [.. pages]);
                    warmedUp.TrySetResult(false);
                }

                if (warmedUp is null || warmedUp.Task.IsCompleted)
                {
                    await Task.Delay(gamesSettings.MediaCachePageDelay, cancellationToken);
                }
            }

            // AniList sometimes answers HTTP 200 with an empty page and no errors while it is degraded.
            if (count == 0)
            {
                logger.LogWarning("AniList returned no media for the cache refresh; the previous one is kept");
                return false;
            }

            mediaCache.Replace(gamesSettings.MediaCachePageFrom, pages);
            logger.LogInformation("AniList media cache refreshed: {Count} entries in {Pages} pages", count, pages.Count);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not refresh the AniList media cache; the previous one is kept");
            return false;
        }
        finally
        {
            warmedUp?.TrySetResult(true);
        }
    }
}

/// <param name="WarmedUp">
/// Completes once the cache holds the warm-up amount: <c>true</c> when the whole crawl already ended (or
/// failed), <c>false</c> when it keeps running in background.
/// </param>
/// <param name="Crawl">Completes when the whole crawl ends: <c>false</c> if it failed or came back empty.</param>
public sealed record MediaCacheWarmUp(Task<bool> WarmedUp, Task<bool> Crawl);
