using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaScoringTests
{
    private sealed record Player(string Name, int Score);

    [Fact]
    public void SortsDescendingAndNumbersFromOne()
    {
        Player[] players = [new("a", 1), new("b", 5), new("c", 3)];

        var ranking = TriviaScoring.Rank(players, j => j.Score, rounds: 10);

        Assert.Equal(["b", "c", "a"], ranking.ConvertAll(r => r.Participant.Name));
        Assert.Equal([1, 2, 3], ranking.ConvertAll(r => r.Position));
    }

    [Fact]
    public void TiesSharePosition()
    {
        Player[] players = [new("a", 5), new("b", 5), new("c", 2)];

        var ranking = TriviaScoring.Rank(players, j => j.Score, rounds: 10);

        Assert.Equal([1, 1, 2], ranking.ConvertAll(r => r.Position));
    }

    [Fact]
    public void ThePercentageUsesIntegerDivision()
    {
        Player[] players = [new("a", 2)];

        var ranking = TriviaScoring.Rank(players, j => j.Score, rounds: 3);

        // 2 * 100 / 3 = 66 (no 66.67 ni 67)
        Assert.Equal(66, ranking.Single().Percentage);
    }

    [Fact]
    public void FirstWithZeroPointsIsStillPositionOne()
    {
        Player[] players = [new("a", 0), new("b", 0)];

        var ranking = TriviaScoring.Rank(players, j => j.Score, rounds: 5);

        Assert.Equal([1, 1], ranking.ConvertAll(r => r.Position));
        Assert.Equal([0, 0], ranking.ConvertAll(r => r.Percentage));
    }

    [Fact]
    public void WithoutParticipantsTheRankingIsEmpty()
    {
        Assert.Empty(TriviaScoring.Rank(Array.Empty<Player>(), j => j.Score, rounds: 5));
    }

    [Fact]
    public void ZeroOrNegativeRounds_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TriviaScoring.Rank<Player>([new("a", 1)], j => j.Score, rounds: 0));
    }

    [Fact]
    public void TotalScore_AddsEverythingUp()
    {
        Player[] players = [new("a", 1), new("b", 5), new("c", 3)];

        Assert.Equal(9, TriviaScoring.TotalScore(players, j => j.Score));
    }
}
