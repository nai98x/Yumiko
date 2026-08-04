using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;

namespace Yumiko.Model.Interfaces;

/// <summary>
/// Acceso a la API de AniList. Las implementaciones traducen los fallos de transporte a
/// <see cref="Exceptions.AnilistApiException"/> y derivados; "no encontrado" se devuelve como
/// <c>null</c> o lista vacía, no como excepción.
/// </summary>
public interface IAnilistClient
{
    Task<List<Media>> SearchMediaAsync(string search, MediaType type, int perPage, CancellationToken cancellationToken = default);

    Task<Media?> GetMediaAsync(int id, MediaType type, int perPage, CancellationToken cancellationToken = default);

    Task<List<Character>> SearchCharacterAsync(string search, int perPage, CancellationToken cancellationToken = default);

    Task<Character?> GetRandomCharacterAsync(int page, CancellationToken cancellationToken = default);

    Task<List<Staff>> SearchStaffAsync(string search, int perPage, CancellationToken cancellationToken = default);

    Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Perfil del dueño del token OAuth (query <c>Viewer</c>).</summary>
    Task<User?> GetViewerAsync(string accessToken, CancellationToken cancellationToken = default);

    Task<MediaUserStatistics?> GetMediaFromUserAsync(int userId, int mediaId, CancellationToken cancellationToken = default);

    Task<MediaUserList?> GetMediaListsAsync(int userId, MediaUserStatus status, MediaUserSort order, MediaTitleType titleLanguage, MediaType type, CancellationToken cancellationToken = default);

    Task<(User? User, MediaListCollection? Recommendations)> GetRecommendationsAsync(int userId, MediaType type, CancellationToken cancellationToken = default);

    /// <summary>Personaje al azar de la página <paramref name="page"/> ordenada por favoritos.</summary>
    Task<CharacterOld?> GetRandomCharacterSimpleAsync(int page, CancellationToken cancellationToken = default);

    /// <summary>Media al azar de la página <paramref name="page"/> ordenada por favoritos.</summary>
    Task<Anime?> GetRandomMediaAsync(int page, MediaType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Página del pool de medias que alimenta los juegos. Los flags de <paramref name="query"/>
    /// deciden qué colecciones anidadas se traen y se mapean.
    /// </summary>
    Task<GameMediaPage> GetGameMediaPageAsync(GameMediaQuery query, int page, CancellationToken cancellationToken = default);

    /// <summary>Página de personajes ordenada por favoritos, para el pool de los juegos.</summary>
    Task<GameCharacterPage> GetGameCharacterPageAsync(int page, CancellationToken cancellationToken = default);

    /// <summary>Lista completa de géneros que maneja AniList.</summary>
    Task<List<string>> GetGenresAsync(CancellationToken cancellationToken = default);

    /// <summary>Estado actual del rate limit, leído de una consulta mínima.</summary>
    Task<AnilistRateLimit> GetRateLimitAsync(CancellationToken cancellationToken = default);
}
