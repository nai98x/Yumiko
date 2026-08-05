using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Application.Tests.Anilist;

/// <summary>
/// Double of <see cref="IAnilistClient"/> for the Application tests. It only implements what the
/// services under test use; the rest throws, so a test that starts calling something new finds out.
/// </summary>
internal sealed class FakeAnilistClient : IAnilistClient
{
    public required Func<int, MediaType, (User?, MediaListCollection?)> Recommendations { get; init; }

    public required Func<int, MediaUserStatus, MediaType, MediaUserList?> MediaLists { get; init; }

    public List<(MediaUserStatus Status, MediaType Type)> ListasPedidas { get; } = [];

    public Task<(User? User, MediaListCollection? Recommendations)> GetRecommendationsAsync(int userId, MediaType type, CancellationToken cancellationToken = default) =>
        Task.FromResult(Recommendations(userId, type));

    public Task<MediaUserList?> GetMediaListsAsync(int userId, MediaUserStatus status, MediaUserSort order, MediaTitleType titleLanguage, MediaType type, CancellationToken cancellationToken = default)
    {
        ListasPedidas.Add((status, type));
        return Task.FromResult(MediaLists(userId, status, type));
    }

    public Task<List<Media>> SearchMediaAsync(string search, MediaType type, int perPage, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<Media?> GetMediaAsync(int id, MediaType type, int perPage, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<List<Character>> SearchCharacterAsync(string search, int perPage, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<Character?> GetRandomCharacterAsync(int page, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<List<Staff>> SearchStaffAsync(string search, int perPage, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<User?> GetViewerAsync(string accessToken, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<MediaUserStatistics?> GetMediaFromUserAsync(int userId, int mediaId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<CharacterOld?> GetRandomCharacterSimpleAsync(int page, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<Anime?> GetRandomMediaAsync(int page, MediaType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<GameMediaPage> GetGameMediaPageAsync(GameMediaQuery query, int page, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<GameCharacterPage> GetGameCharacterPageAsync(int page, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<List<string>> GetGenresAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<AnilistRateLimit> GetRateLimitAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
}
