using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Services;

/// <summary>
/// Fills the media cache used by Higher or Lower and the trivia. It is called by the startup
/// (<c>GuildDownloadCompleted</c>) and by the daily task.
/// </summary>
public sealed class MediaCacheRefresher(
    AnilistMediaCacheState mediaCache,
    IAnilistClient anilistClient,
    GamesSettings gamesSettings,
    ILogger<MediaCacheRefresher> logger)
{
    /// <summary>
    /// The swap is atomic: until the crawl finishes, the games keep seeing the previous
    /// cache. If the query fails nothing is touched, so an AniList outage does not empty the pool.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
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
            }

            mediaCache.Replace(media);
            logger.LogInformation("AniList media cache refreshed: {Count} entries", media.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not refresh the AniList media cache; the previous one is kept");
        }
    }
}
