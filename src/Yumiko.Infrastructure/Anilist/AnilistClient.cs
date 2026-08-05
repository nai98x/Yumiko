using GraphQL;
using Yumiko.Infrastructure.Anilist.Responses;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;
using Yumiko.Model.Exceptions;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.Anilist;

internal sealed class AnilistClient(AnilistGraphQLExecutor executor) : IAnilistClient
{
    public async Task<List<Media>> SearchMediaAsync(string search, MediaType type, int perPage, CancellationToken cancellationToken = default)
    {
        AnilistResponse<MediaPageResponse> response = await executor.SendQueryAsync<MediaPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.MediaSearch,
                Variables = new { search, type = System.Enum.GetName(type), perPage },
            },
            cancellationToken);

        return response.Data?.Page?.Media ?? [];
    }

    public async Task<Media?> GetMediaAsync(int id, MediaType type, int perPage, CancellationToken cancellationToken = default)
    {
        AnilistResponse<MediaPageResponse> response = await executor.SendQueryAsync<MediaPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.MediaById,
                Variables = new { id, type = System.Enum.GetName(type), perPage },
            },
            cancellationToken);

        return response.Data?.Page?.Media?.FirstOrDefault();
    }

    public async Task<List<Character>> SearchCharacterAsync(string search, int perPage, CancellationToken cancellationToken = default)
    {
        AnilistResponse<CharacterPageResponse> response = await executor.SendQueryAsync<CharacterPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.CharacterSearch,
                Variables = new { search, perPage },
            },
            cancellationToken);

        return response.Data?.Page?.Characters ?? [];
    }

    public async Task<Character?> GetRandomCharacterAsync(int page, CancellationToken cancellationToken = default)
    {
        AnilistResponse<CharacterPageResponse> response = await executor.SendQueryAsync<CharacterPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.RandomCharacter,
                Variables = new { page },
            },
            cancellationToken);

        return response.Data?.Page?.Characters?.FirstOrDefault();
    }

    public async Task<List<Staff>> SearchStaffAsync(string search, int perPage, CancellationToken cancellationToken = default)
    {
        AnilistResponse<StaffPageResponse> response = await executor.SendQueryAsync<StaffPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.StaffSearch,
                Variables = new { search, perPage },
            },
            cancellationToken);

        return response.Data?.Page?.Staffs ?? [];
    }

    public async Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        AnilistResponse<ProfileResponse> response = await executor.SendQueryAsync<ProfileResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.Profile,
                Variables = new { code = userId },
            },
            cancellationToken);

        return response.Data?.User;
    }

    public async Task<User?> GetViewerAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        AnilistResponse<ViewerResponse> response = await executor.SendAuthenticatedQueryAsync<ViewerResponse>(
            new GraphQLRequest { Query = AnilistQueries.Viewer },
            accessToken,
            cancellationToken);

        return response.Data?.Viewer;
    }

    public async Task<MediaUserStatistics?> GetMediaFromUserAsync(int userId, int mediaId, CancellationToken cancellationToken = default)
    {
        try
        {
            AnilistResponse<MediaListResponse> response = await executor.SendQueryAsync<MediaListResponse>(
                new GraphQLRequest
                {
                    Query = AnilistQueries.MediaUser,
                    Variables = new { userId, mediaId },
                },
                cancellationToken);

            return response.Data?.MediaList;
        }
        catch (AnilistNotFoundException)
        {
            // The user has no entry for that media: it is a valid answer, not a failure.
            return null;
        }
    }

    public async Task<MediaUserList?> GetMediaListsAsync(int userId, MediaUserStatus status, MediaUserSort order, MediaTitleType titleLanguage, MediaType type, CancellationToken cancellationToken = default)
    {
        string sort = System.Enum.GetName(order)!;

        // MEDIA_TITLE_DESC does not exist on AniList: it has to be resolved to the requested title language.
        if (sort == "MEDIA_TITLE_DESC")
        {
            sort = titleLanguage switch
            {
                MediaTitleType.ROMAJI => "MEDIA_TITLE_ROMAJI",
                MediaTitleType.ENGLISH => "MEDIA_TITLE_ENGLISH",
                MediaTitleType.NATIVE => "MEDIA_TITLE_NATIVE",
                _ => throw new ArgumentOutOfRangeException(nameof(titleLanguage)),
            };
        }

        try
        {
            AnilistResponse<MediaUserListResponse> response = await executor.SendQueryAsync<MediaUserListResponse>(
                new GraphQLRequest
                {
                    Query = AnilistQueries.MediaList,
                    Variables = new { userId, status = System.Enum.GetName(status), sort, type = System.Enum.GetName(type) },
                },
                cancellationToken);

            return response.Data?.MediaListCollection?.Lists?.FirstOrDefault();
        }
        catch (AnilistNotFoundException)
        {
            // The user has no list for that status/type.
            return null;
        }
    }

    public async Task<(User? User, MediaListCollection? Recommendations)> GetRecommendationsAsync(int userId, MediaType type, CancellationToken cancellationToken = default)
    {
        AnilistResponse<RecommendationsResponse> response = await executor.SendQueryAsync<RecommendationsResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.Recommendations,
                Variables = new { userId, type = System.Enum.GetName(type) },
            },
            cancellationToken);

        return (response.Data?.User, response.Data?.Recommendations);
    }

    public async Task<CharacterOld?> GetRandomCharacterSimpleAsync(int page, CancellationToken cancellationToken = default)
    {
        AnilistResponse<SimpleCharacterPageResponse> response = await executor.SendQueryAsync<SimpleCharacterPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.RandomCharacterSimple,
                Variables = new { page },
            },
            cancellationToken);

        SimpleCharacter? character = response.Data?.Page?.Characters?.FirstOrDefault();
        if (character is null)
        {
            return null;
        }

        SimpleMedia? mainMedia = character.Media?.Nodes?.FirstOrDefault();

        return new CharacterOld
        {
            NameFull = character.Name?.Full,
            Image = character.Image?.Large,
            SiteUrl = character.SiteUrl,
            Favourites = character.Favourites,
            MainAnime = new Anime
            {
                TitleRomaji = mainMedia?.Title?.Romaji!,
                SiteUrl = mainMedia?.SiteUrl,
            },
        };
    }

    public async Task<Anime?> GetRandomMediaAsync(int page, MediaType type, CancellationToken cancellationToken = default)
    {
        AnilistResponse<SimpleMediaPageResponse> response = await executor.SendQueryAsync<SimpleMediaPageResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.RandomMediaSimple,
                Variables = new { page, type = System.Enum.GetName(type) },
            },
            cancellationToken);

        SimpleMedia? media = response.Data?.Page?.Media?.FirstOrDefault();
        if (media is null)
        {
            return null;
        }

        return new Anime
        {
            TitleRomaji = media.Title?.Romaji!,
            TitleEnglish = media.Title?.English,
            Image = media.CoverImage?.Large,
            SiteUrl = media.SiteUrl,
            Favourites = media.Favourites,
        };
    }

    public async Task<GameMediaPage> GetGameMediaPageAsync(GameMediaQuery query, int page, CancellationToken cancellationToken = default)
    {
        AnilistResponse<GamePoolResponse> response = await executor.SendQueryAsync<GamePoolResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.GamePool(GameMediaMapper.Filtros(query)),
                Variables = new { page },
            },
            cancellationToken);

        return GameMediaMapper.Map(response.Data, query);
    }

    public async Task<GameCharacterPage> GetGameCharacterPageAsync(int page, CancellationToken cancellationToken = default)
    {
        AnilistResponse<CharacterPoolResponse> response = await executor.SendQueryAsync<CharacterPoolResponse>(
            new GraphQLRequest
            {
                Query = AnilistQueries.CharacterPool,
                Variables = new { page },
            },
            cancellationToken);

        CharacterPoolPage? pageData = response.Data?.Page;

        return new GameCharacterPage
        {
            HasNextPage = pageData?.PageInfo?.HasNextPage ?? false,
            Characters = [.. (pageData?.Characters ?? []).Select(c =>
            {
                CharacterPoolMediaNode? mainMedia = c.Media?.Nodes?.FirstOrDefault();
                return new CharacterOld
                {
                    Image = c.Image?.Large,
                    NameFirst = c.Name?.First,
                    NameLast = c.Name?.Last,
                    NameFull = c.Name?.Full,
                    SiteUrl = c.SiteUrl,
                    Favourites = c.Favourites,
                    MainAnime = new Anime
                    {
                        TitleRomaji = mainMedia?.Title?.Romaji!,
                        SiteUrl = mainMedia?.SiteUrl,
                    },
                };
            })],
        };
    }

    public async Task<List<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        AnilistResponse<GenreCollectionResponse> response = await executor.SendQueryAsync<GenreCollectionResponse>(
            new GraphQLRequest { Query = AnilistQueries.GenreCollection },
            cancellationToken);

        return response.Data?.GenreCollection ?? [];
    }

    public async Task<AnilistRateLimit> GetRateLimitAsync(CancellationToken cancellationToken = default)
    {
        AnilistResponse<RateLimitProbeResponse> response = await executor.SendQueryAsync<RateLimitProbeResponse>(
            new GraphQLRequest { Query = AnilistQueries.RateLimitProbe },
            cancellationToken);

        return response.RateLimit;
    }
}
