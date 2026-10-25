using Yumiko.Application.Games;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Games;

/// <summary>
/// Fetches the pool of media and characters that feeds the games.
/// </summary>
public sealed class GamePool(IAnilistClient anilist, AnilistMediaCacheState mediaCache, GamesSettings gamesSettings)
{
    public async Task<List<Anime>> GetMediaAsync(GameMediaQuery query, int pageFrom, int pageTo, CancellationToken cancellationToken = default)
    {
        (int first, int second) = MediaPoolBuilder.PickPages(pageFrom, pageTo);

        if (IsServedByCache(query) && mediaCache.GetPages(pageFrom, pageTo, [first, second]) is { } cached)
        {
            return cached;
        }

        List<Anime> media = [];

        foreach (int pageNumber in new[] { first, second })
        {
            GameMediaPage page = await anilist.GetGameMediaPageAsync(query, pageNumber, cancellationToken);
            media.AddRange(page.Media);

            if (!page.HasNextPage)
            {
                break;
            }
        }

        return media;
    }

    /// <summary>
    /// The cache is crawled with the same ranking (anime by favourites) but without adult media, so it
    /// only replaces queries that do not need it.
    /// </summary>
    private static bool IsServedByCache(GameMediaQuery query) =>
        query is { Type: MediaType.ANIME, Genre: null, IncludeAdult: false };

    /// <summary>
    /// In genres mode the page range depends on how many the genre has, so the first page has to be
    /// queried before drawing.
    /// </summary>
    public async Task<(int PageFrom, int PageTo)> GetGenreRangeAsync(GameMediaQuery query, CancellationToken cancellationToken = default)
    {
        GameMediaPage first = await anilist.GetGameMediaPageAsync(query, 1, cancellationToken);
        return MediaPoolBuilder.GenreRange(first.LastPage, gamesSettings.GenrePageDepth);
    }

    public async Task<List<CharacterOld>> GetCharactersAsync(int pageFrom, int pageTo, CancellationToken cancellationToken = default)
    {
        (int first, int second) = MediaPoolBuilder.PickPages(pageFrom, pageTo);
        List<CharacterOld> characters = [];

        foreach (int pageNumber in new[] { first, second })
        {
            GameCharacterPage page = await anilist.GetGameCharacterPageAsync(pageNumber, cancellationToken);
            characters.AddRange(page.Characters);

            if (!page.HasNextPage)
            {
                break;
            }
        }

        return characters;
    }

    public Task<List<string>> GetGenresAsync(bool includeHentai, CancellationToken cancellationToken = default) =>
        FilterGenresAsync(includeHentai, cancellationToken);

    private async Task<List<string>> FilterGenresAsync(bool includeHentai, CancellationToken cancellationToken)
    {
        List<string> genres = await anilist.GetGenresAsync(cancellationToken);

        return includeHentai
            ? genres
            : [.. genres.Where(g => !string.Equals(g.Trim(), "hentai", StringComparison.OrdinalIgnoreCase))];
    }
}
