using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;

namespace Yumiko.Model.Interfaces;

/// <summary>
/// Access to the AniList API. Implementations translate transport failures into
/// <see cref="Exceptions.AnilistApiException"/> and derivatives; "not found" is returned as
/// <c>null</c> or an empty list, not as an exception.
/// </summary>
public interface IAnilistClient
{
    Task<List<Media>> SearchMediaAsync(string search, MediaType type, int perPage, CancellationToken cancellationToken = default);

    Task<Media?> GetMediaAsync(int id, MediaType type, int perPage, CancellationToken cancellationToken = default);

    Task<List<Character>> SearchCharacterAsync(string search, int perPage, CancellationToken cancellationToken = default);

    Task<Character?> GetRandomCharacterAsync(int page, CancellationToken cancellationToken = default);

    Task<List<Staff>> SearchStaffAsync(string search, int perPage, CancellationToken cancellationToken = default);

    Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Profile of the owner of the OAuth token (<c>Viewer</c> query).</summary>
    Task<User?> GetViewerAsync(string accessToken, CancellationToken cancellationToken = default);

    Task<MediaUserStatistics?> GetMediaFromUserAsync(int userId, int mediaId, CancellationToken cancellationToken = default);

    Task<MediaUserList?> GetMediaListsAsync(int userId, MediaUserStatus status, MediaUserSort order, MediaTitleType titleLanguage, MediaType type, CancellationToken cancellationToken = default);

    Task<(User? User, MediaListCollection? Recommendations)> GetRecommendationsAsync(int userId, MediaType type, CancellationToken cancellationToken = default);

    /// <summary>Random character from page <paramref name="page"/> sorted by favourites.</summary>
    Task<CharacterOld?> GetRandomCharacterSimpleAsync(int page, CancellationToken cancellationToken = default);

    /// <summary>Random media from page <paramref name="page"/> sorted by favourites.</summary>
    Task<Anime?> GetRandomMediaAsync(int page, MediaType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Page of the media pool that feeds the games. The flags of <paramref name="query"/>
    /// decide which nested collections are fetched and mapped.
    /// </summary>
    Task<GameMediaPage> GetGameMediaPageAsync(GameMediaQuery query, int page, CancellationToken cancellationToken = default);

    /// <summary>Page of characters sorted by favourites, for the games pool.</summary>
    Task<GameCharacterPage> GetGameCharacterPageAsync(int page, CancellationToken cancellationToken = default);

    /// <summary>Full list of genres AniList handles.</summary>
    Task<List<string>> GetGenresAsync(CancellationToken cancellationToken = default);

    /// <summary>Current rate limit state, read from a minimal query.</summary>
    Task<AnilistRateLimit> GetRateLimitAsync(CancellationToken cancellationToken = default);
}
