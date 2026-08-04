using Yumiko.Model.Entities;

namespace Yumiko.Bot.Services.State;

/// <summary>
/// Caché de medias de AniList que alimenta Higher or Lower y la trivia. El intercambio es atómico:
/// el refresco tarda varios minutos y durante ese lapso los juegos siguen viendo el pool anterior.
/// </summary>
public sealed class AnilistMediaCacheState
{
    private volatile IReadOnlyList<Anime> _media = [];

    public IReadOnlyList<Anime> Media => _media;

    public bool IsEmpty => _media.Count == 0;

    public void Replace(IReadOnlyList<Anime> media) => _media = media;
}
