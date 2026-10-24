using Yumiko.Model.Entities;

namespace Yumiko.Bot.Services.State;

/// <summary>
/// Cache of AniList media that feeds Higher or Lower and the anime-based trivia modes. The swap is
/// atomic: the daily refresh takes several minutes and during that window the games keep seeing the
/// previous pool. At startup it is published early and grows while the first crawl finishes.
/// </summary>
public sealed class AnilistMediaCacheState
{
    private sealed record Snapshot(int FirstPage, IReadOnlyList<IReadOnlyList<Anime>> Pages, IReadOnlyList<Anime> Media);

    private volatile Snapshot _snapshot = new(0, [], []);

    public IReadOnlyList<Anime> Media => _snapshot.Media;

    public bool IsEmpty => _snapshot.Media.Count == 0;

    /// <summary>
    /// Pages as AniList returned them, the first one being <paramref name="firstPage"/>; they keep the
    /// position of each media in the ranking, which is what the trivia difficulty is based on.
    /// </summary>
    public void Replace(int firstPage, IReadOnlyList<IReadOnlyList<Anime>> pages) =>
        _snapshot = new Snapshot(firstPage, pages, [.. pages.SelectMany(p => p)]);

    /// <summary>
    /// Media of the given AniList pages, or <c>null</c> if the cache does not hold every page of the
    /// range <paramref name="pageFrom"/>-<paramref name="pageTo"/> (still loading, or out of the crawl).
    /// </summary>
    public List<Anime>? GetPages(int pageFrom, int pageTo, IEnumerable<int> pageNumbers)
    {
        Snapshot snapshot = _snapshot;
        int lastPage = snapshot.FirstPage + snapshot.Pages.Count - 1;

        if (snapshot.Pages.Count == 0 || pageFrom < snapshot.FirstPage || pageTo > lastPage)
        {
            return null;
        }

        return [.. pageNumbers.SelectMany(n => snapshot.Pages[n - snapshot.FirstPage])];
    }
}
