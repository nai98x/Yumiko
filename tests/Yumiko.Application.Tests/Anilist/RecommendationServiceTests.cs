using Yumiko.Application.Anilist;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Anilist;

public class RecommendationServiceTests
{
    private static User Profile(decimal meanScore, decimal deviation, string language = "ROMAJI") => new()
    {
        Id = 42,
        Name = "nai",
        Options = new ProfileOptions { TitleLanguage = language },
        Statistics = new ProfileStatistics
        {
            Anime = new ProfileStatisticsAnime { MeanScore = meanScore, StandardDeviation = deviation },
            Manga = new ProfileStatisticsManga { MeanScore = meanScore + 1, StandardDeviation = deviation + 1 },
        },
    };

    private static MediaListCollection Collection(int mediaId, int score, int recommendedId, string title) =>
        new()
        {
            Lists =
            [
                new MediaList
                {
                    Entries =
                    [
                        new MediaEntry
                        {
                            MediaId = mediaId,
                            Score = score,
                            Media = new MediaRecommendations
                            {
                                Recommendations = new RecommendationsConnection
                                {
                                    Nodes =
                                    [
                                        new RecommendationNode
                                        {
                                            Rating = 50,
                                            MediaRecommendation = new Media
                                            {
                                                Id = recommendedId,
                                                Title = new MediaTitle { Romaji = title },
                                            },
                                        },
                                    ],
                                },
                            },
                        },
                    ],
                },
            ],
        };

    private static MediaUserList ListWith(params int[] mediaIds) => new()
    {
        Name = "Completed",
        Entries = [.. mediaIds.Select(id => new MediaUserEntry { Media = new Media { Id = id, Title = new MediaTitle { Romaji = $"m{id}" } } })],
    };

    [Fact]
    public async Task GetAsync_WithoutProfileReturnsAnEmptyList()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (null, null),
            MediaLists = (_, _, _) => null,
        };

        (User? profile, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        Assert.Null(profile);
        Assert.Empty(recommendations);
    }

    [Fact]
    public async Task GetAsync_WithoutCollectionReturnsTheProfileButNoRecommendations()
    {
        User profile = Profile(7, 1);
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (profile, null),
            MediaLists = (_, _, _) => null,
        };

        (User? devuelto, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        Assert.Same(profile, devuelto);
        Assert.Empty(recommendations);
        Assert.Empty(client.ListasPedidas);
    }

    [Fact]
    public async Task GetAsync_FetchesTheOtherTypeListsToExcludeWhatWasAlreadySeen()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 1), Collection(1, 10, 100, "Steins;Gate")),
            MediaLists = (_, _, _) => null,
        };

        await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        // Para recomendaciones de anime se consultan las listas de MANGA, no las de anime.
        Assert.Equal(
            [(MediaUserStatus.COMPLETED, MediaType.MANGA), (MediaUserStatus.CURRENT, MediaType.MANGA)],
            client.ListasPedidas);
    }

    [Fact]
    public async Task GetAsync_ForMangaQueriesTheAnimeLists()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 1), Collection(1, 10, 100, "Berserk")),
            MediaLists = (_, _, _) => null,
        };

        await new RecommendationService(client).GetAsync(42, MediaType.MANGA);

        Assert.All(client.ListasPedidas, pedido => Assert.Equal(MediaType.ANIME, pedido.Type));
    }

    [Fact]
    public async Task GetAsync_ScoresWithTheStatisticsOfTheRequestedType()
    {
        // Anime: media 7, desvío 1 => adjustedScore (10-7)/1 = 3, peso 2 => 6.
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 1), Collection(1, 10, 100, "Steins;Gate")),
            MediaLists = (_, _, _) => null,
        };

        (_, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        AnimeRecommendation recomendacion = Assert.Single(recommendations);
        Assert.Equal(100, recomendacion.Id);
        Assert.Equal("Steins;Gate", recomendacion.Title);
        Assert.Equal(6m, recomendacion.Score);
    }

    [Fact]
    public async Task GetAsync_DropsWhatTheUserAlreadyHasInTheOtherTypeList()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 1), Collection(1, 10, 100, "Steins;Gate")),
            MediaLists = (_, status, _) => status == MediaUserStatus.COMPLETED ? ListWith(100) : null,
        };

        (_, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        Assert.Empty(recommendations);
    }

    [Fact]
    public async Task GetAsync_AlsoDropsWhatIsInProgress()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 1), Collection(1, 10, 100, "Steins;Gate")),
            MediaLists = (_, status, _) => status == MediaUserStatus.CURRENT ? ListWith(100) : null,
        };

        (_, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        Assert.Empty(recommendations);
    }

    [Fact]
    public async Task GetAsync_WithZeroDeviationRecommendsNothing()
    {
        FakeAnilistClient client = new()
        {
            Recommendations = (_, _) => (Profile(7, 0), Collection(1, 10, 100, "Steins;Gate")),
            MediaLists = (_, _, _) => null,
        };

        (User? profile, List<AnimeRecommendation> recommendations) =
            await new RecommendationService(client).GetAsync(42, MediaType.ANIME);

        Assert.NotNull(profile);
        Assert.Empty(recommendations);
    }
}
