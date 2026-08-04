using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Services;

/// <summary>
/// Rellena el caché de medias que usan Higher or Lower y la trivia. Lo llaman el arranque
/// (<c>GuildDownloadCompleted</c>) y la tarea diaria.
/// </summary>
public sealed class MediaCacheRefresher(
    AnilistMediaCacheState mediaCache,
    IAnilistClient anilistClient,
    GamesSettings gamesSettings,
    ILogger<MediaCacheRefresher> logger)
{
    /// <summary>
    /// El intercambio es atómico: hasta que termina el crawl, los juegos siguen viendo el caché
    /// anterior. Si la consulta falla no se toca nada, así un AniList caído no vacía el pool.
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
            logger.LogInformation("Caché de media de AniList actualizado: {Cantidad} entradas", media.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo actualizar el caché de media de AniList; queda el anterior");
        }
    }
}
