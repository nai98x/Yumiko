using Yumiko.Model.Entities;

namespace Yumiko.Bot.Services.State;

/// <summary>
/// Cache of AniList media that feeds Higher or Lower. The swap is atomic: the daily refresh takes
/// several minutes and during that window the games keep seeing the previous pool. At startup it is
/// published early and grows while the first crawl finishes.
/// </summary>
public sealed class AnilistMediaCacheState
{
    private volatile IReadOnlyList<Anime> _media = [];

    public IReadOnlyList<Anime> Media => _media;

    public bool IsEmpty => _media.Count == 0;

    public void Replace(IReadOnlyList<Anime> media) => _media = media;
}
