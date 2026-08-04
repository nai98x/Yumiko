using Yumiko.Application.Anilist;
using Yumiko.Model.Entities.Anilist;

namespace Yumiko.Application.Tests.Anilist;

public class RecommendationScoringTests
{
    private static MediaListCollection Collection(params MediaEntry[] entries) =>
        new() { Lists = [new MediaList { Entries = [.. entries] }] };

    private static MediaEntry Entry(int mediaId, int score, params (int Id, string Title, int Rating)[] recommendations) =>
        new()
        {
            MediaId = mediaId,
            Score = score,
            Media = new MediaRecommendations
            {
                Recommendations = new RecommendationsConnection
                {
                    Nodes = [.. recommendations.Select(r => new RecommendationNode
                    {
                        Rating = r.Rating,
                        MediaRecommendation = new Media
                        {
                            Id = r.Id,
                            Title = new MediaTitle { Romaji = r.Title },
                        },
                    })],
                },
            },
        };

    [Fact]
    public void ZeroDeviation_RecommendsNothing()
    {
        var collection = Collection(Entry(1, 10, (100, "Steins;Gate", 50)));

        var result = RecommendationScoring.Score(collection, meanScore: 7, standardDeviation: 0, false, new HashSet<int>());

        Assert.Empty(result);
    }

    [Fact]
    public void NullCollection_RecommendsNothing()
    {
        Assert.Empty(RecommendationScoring.Score(null, 7, 1, false, new HashSet<int>()));
    }

    [Fact]
    public void AccumulatesTheScoreOfSeveralEntriesPointingAtTheSameMedia()
    {
        // adjustedScore = (10 - 7) / 1 = 3 por entrada; peso 2 => 6 cada una, 12 en total.
        var collection = Collection(
            Entry(1, 10, (100, "Steins;Gate", 50)),
            Entry(2, 10, (100, "Steins;Gate", 50)));

        var result = RecommendationScoring.Score(collection, meanScore: 7, standardDeviation: 1, false, new HashSet<int>());

        var recomendacion = Assert.Single(result);
        Assert.Equal(100, recomendacion.Id);
        Assert.Equal(12m, recomendacion.Score);
    }

    [Fact]
    public void FiltersOutWhatTheUserAlreadyHasInThisList()
    {
        var collection = Collection(
            Entry(1, 10, (100, "Steins;Gate", 50)),
            Entry(100, 9));

        Assert.Empty(RecommendationScoring.Score(collection, 7, 1, false, new HashSet<int>()));
    }

    [Fact]
    public void FiltersOutTheExcludedIdsFromTheOtherTypeList()
    {
        var collection = Collection(Entry(1, 10, (100, "Steins;Gate", 50)));

        Assert.Empty(RecommendationScoring.Score(collection, 7, 1, false, new HashSet<int> { 100 }));
    }

    [Fact]
    public void IgnoresEntriesWithoutAScore()
    {
        var collection = Collection(Entry(1, 0, (100, "Steins;Gate", 50)));

        Assert.Empty(RecommendationScoring.Score(collection, 7, 1, false, new HashSet<int>()));
    }

    [Fact]
    public void IgnoresNodesWithoutARating()
    {
        var collection = Collection(Entry(1, 10, (100, "Steins;Gate", 0)));

        Assert.Empty(RecommendationScoring.Score(collection, 7, 1, false, new HashSet<int>()));
    }

    [Fact]
    public void AppliesTheThresholdOfThree()
    {
        // adjustedScore = (8 - 7) / 1 = 1; peso 2 => 2, por debajo del umbral de 3.
        var justoDebajo = Collection(Entry(1, 8, (100, "A", 50)));
        Assert.Empty(RecommendationScoring.Score(justoDebajo, 7, 1, false, new HashSet<int>()));

        // (9 - 7) / 1 = 2; peso 2 => 4, por encima del umbral.
        var justoArriba = Collection(Entry(1, 9, (100, "A", 50)));
        Assert.Single(RecommendationScoring.Score(justoArriba, 7, 1, false, new HashSet<int>()));
    }

    [Fact]
    public void SortsByScoreDescending()
    {
        var collection = Collection(
            Entry(1, 10, (100, "Flojo", 50)),
            Entry(2, 10, (200, "Fuerte", 50)),
            Entry(3, 10, (200, "Fuerte", 50)));

        var result = RecommendationScoring.Score(collection, 7, 1, false, new HashSet<int>());

        Assert.Equal([200, 100], result.ConvertAll(r => r.Id));
    }

    [Fact]
    public void UsesTheEnglishTitleOnlyWhenTheProfilePrefersItAndItExists()
    {
        var collection = new MediaListCollection
        {
            Lists =
            [
                new MediaList
                {
                    Entries =
                    [
                        new MediaEntry
                        {
                            MediaId = 1,
                            Score = 10,
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
                                                Id = 100,
                                                Title = new MediaTitle { Romaji = "Kimi no Na wa", English = "Your Name" },
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

        Assert.Equal("Your Name", RecommendationScoring.Score(collection, 7, 1, preferEnglishTitle: true, new HashSet<int>()).Single().Title);
        Assert.Equal("Kimi no Na wa", RecommendationScoring.Score(collection, 7, 1, preferEnglishTitle: false, new HashSet<int>()).Single().Title);
    }

    [Fact]
    public void TheRatingWeightIsAlwaysTwoExceptForRatingOne()
    {
        // 2 - (1 / rating) en aritmética entera: da 1 solo con rating 1 y 2 con cualquier otro.
        // Cambiar el peso re-ordena las recomendaciones, así que queda fijado acá.
        var ratingUno = Collection(Entry(1, 10, (100, "A", 1)));
        var ratingCien = Collection(Entry(1, 10, (100, "A", 100)));

        Assert.Equal(3m, RecommendationScoring.Score(ratingUno, 7, 1, false, new HashSet<int>()).Single().Score);
        Assert.Equal(6m, RecommendationScoring.Score(ratingCien, 7, 1, false, new HashSet<int>()).Single().Score);
    }

    [Fact]
    public void MinimumScore_IsThree()
    {
        // El umbral define qué se muestra: bajarlo llena el embed de ruido, subirlo lo vacía.
        Assert.Equal(3m, RecommendationScoring.MinimumScore);
    }
}
