using Yumiko.Application.Games;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

public class HigherOrLowerTests
{
    private static Anime Media(int id, int score, int favourites) =>
        new() { Id = id, TitleRomaji = $"media-{id}", AvarageScore = score, Favourites = favourites };

    [Fact]
    public void PickPair_FewerThanTwo_ReturnsNull()
    {
        Assert.Null(HigherOrLower.PickPair([], GamemodeHoL.Score, 0.3));
        Assert.Null(HigherOrLower.PickPair([Media(1, 80, 100)], GamemodeHoL.Score, 0.3));
        Assert.Null(HigherOrLower.PickPair([Media(1, 80, 100)], GamemodeHoL.Popularity, 0.3));
    }

    [Fact]
    public void PickPair_NeverRepeatsTheSameMedia()
    {
        List<Anime> list = [.. Enumerable.Range(1, 5).Select(i => Media(i, 70 + i, i))];
        Random rnd = new(12345);

        for (int i = 0; i < 200; i++)
        {
            var pair = HigherOrLower.PickPair(list, GamemodeHoL.Popularity, 0.3, rnd);

            Assert.NotNull(pair);
            Assert.NotEqual(pair.Value.First.Id, pair.Value.Second.Id);
        }
    }

    [Fact]
    public void PickPair_Score_NeverPicksMediaCloserThanTheMinimumDifference()
    {
        List<Anime> list = [.. Enumerable.Range(0, 30).Select(i => Media(i, 60 + i, 0))];
        Random rnd = new(12345);

        for (int i = 0; i < 500; i++)
        {
            var pair = HigherOrLower.PickPair(list, GamemodeHoL.Score, 0.3, rnd);

            Assert.NotNull(pair);
            Assert.True(Math.Abs(pair.Value.First.AvarageScore - pair.Value.Second.AvarageScore) >= 3);
        }
    }

    [Fact]
    public void PickPair_Score_OnlyNearTies_ReturnsNull()
    {
        List<Anime> list = [Media(1, 80, 0), Media(2, 81, 0), Media(3, 82, 0), Media(4, 80, 0)];

        Assert.Null(HigherOrLower.PickPair(list, GamemodeHoL.Score, 0.3));
        Assert.False(HigherOrLower.HasPair(list, GamemodeHoL.Score, 0.3));
    }

    [Fact]
    public void PickPair_Popularity_IgnoresTheScoreDifference()
    {
        List<Anime> list = [Media(1, 80, 10), Media(2, 80, 20)];

        Assert.NotNull(HigherOrLower.PickPair(list, GamemodeHoL.Popularity, 0.3));
        Assert.True(HigherOrLower.HasPair(list, GamemodeHoL.Popularity, 0.3));
    }

    [Theory]
    [InlineData(80, 80, false)]
    [InlineData(80, 81, false)]
    [InlineData(80, 82, false)]
    [InlineData(80, 83, true)]
    [InlineData(83, 80, true)]
    [InlineData(50, 90, true)]
    public void IsValidPair_Score_RequiresTheMinimumDifference(int firstScore, int secondScore, bool expected)
    {
        Assert.Equal(expected, HigherOrLower.IsValidPair(Media(1, firstScore, 0), Media(2, secondScore, 0), GamemodeHoL.Score, 0.3));
    }

    [Fact]
    public void IsValidPair_SameMedia_IsNeverValid()
    {
        Anime anime = Media(1, 80, 10);

        Assert.False(HigherOrLower.IsValidPair(anime, anime, GamemodeHoL.Score, 0));
        Assert.False(HigherOrLower.IsValidPair(anime, anime, GamemodeHoL.Popularity, 0.3));
    }

    [Theory]
    [InlineData(GamemodeHoL.Score, 85, 70, true)]
    [InlineData(GamemodeHoL.Score, 70, 85, false)]
    [InlineData(GamemodeHoL.Popularity, 500, 100, true)]
    [InlineData(GamemodeHoL.Popularity, 100, 500, false)]
    public void IsCorrect_ComparesTheGamemodeValue(GamemodeHoL gamemode, int selectedValue, int otherValue, bool expected)
    {
        Anime selected = gamemode == GamemodeHoL.Score ? Media(1, selectedValue, 0) : Media(1, 0, selectedValue);
        Anime other = gamemode == GamemodeHoL.Score ? Media(2, otherValue, 0) : Media(2, 0, otherValue);

        Assert.Equal(expected, HigherOrLower.IsCorrect(selected, other, gamemode));
    }

    [Fact]
    public void ATieCountsAsCorrect()
    {
        Assert.True(HigherOrLower.IsCorrect(Media(1, 80, 0), Media(2, 80, 0), GamemodeHoL.Score));
        Assert.True(HigherOrLower.IsCorrect(Media(1, 0, 50), Media(2, 0, 50), GamemodeHoL.Popularity));
    }

    [Fact]
    public void ComparedValue_ReturnsTheGamemodeField()
    {
        Anime anime = Media(1, 85, 500);

        Assert.Equal(85, HigherOrLower.ComparedValue(anime, GamemodeHoL.Score));
        Assert.Equal(500, HigherOrLower.ComparedValue(anime, GamemodeHoL.Popularity));
    }

    [Fact]
    public void ScoreOutOfTen_ConvertsFromHundredToTen()
    {
        Assert.Equal(8.5, HigherOrLower.ScoreOutOfTen(Media(1, 85, 0)));
        Assert.Equal(0, HigherOrLower.ScoreOutOfTen(Media(1, 0, 0)));
    }

    [Fact]
    public void ComparedValue_InvalidGamemode_Throws()
    {
        Anime anime = new() { Id = 1, TitleRomaji = "x", AvarageScore = 80, Favourites = 10 };

        Assert.Throws<ArgumentOutOfRangeException>(() => HigherOrLower.ComparedValue(anime, (GamemodeHoL)99));
    }
}
