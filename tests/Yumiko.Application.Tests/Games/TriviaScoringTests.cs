using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaScoringTests
{
    private sealed record Player(string Name, int Score, double Seconds = 0)
    {
        public TimeSpan Time => TimeSpan.FromSeconds(Seconds);
    }

    private static List<TriviaRank<Player>> Rank(IEnumerable<Player> players, int rounds) =>
        TriviaScoring.Rank(players, p => p.Score, p => p.Time, rounds);

    [Fact]
    public void SortsDescendingAndNumbersFromOne()
    {
        Player[] players = [new("a", 1), new("b", 5), new("c", 3)];

        var ranking = Rank(players, rounds: 10);

        Assert.Equal(["b", "c", "a"], ranking.ConvertAll(r => r.Participant.Name));
        Assert.Equal([1, 2, 3], ranking.ConvertAll(r => r.Position));
    }

    [Fact]
    public void TiesInScoreAndTimeSharePosition()
    {
        Player[] players = [new("a", 5, 10), new("b", 5, 10), new("c", 2)];

        var ranking = Rank(players, rounds: 10);

        Assert.Equal([1, 1, 2], ranking.ConvertAll(r => r.Position));
    }

    [Fact]
    public void ATieInScoreGoesToTheLowestAverageTime()
    {
        // b: 4 hits in 20s (5s each); a: 4 hits in 12s (3s each).
        Player[] players = [new("b", 4, 20), new("a", 4, 12), new("c", 1, 1)];

        var ranking = Rank(players, rounds: 10);

        Assert.Equal(["a", "b", "c"], ranking.ConvertAll(r => r.Participant.Name));
        Assert.Equal([1, 2, 3], ranking.ConvertAll(r => r.Position));
    }

    [Fact]
    public void TheScoreWeighsMoreThanTheTime()
    {
        Player[] players = [new("fast", 1, 1), new("slow", 2, 60)];

        var ranking = Rank(players, rounds: 10);

        Assert.Equal(["slow", "fast"], ranking.ConvertAll(r => r.Participant.Name));
    }

    [Fact]
    public void TheAverageIsPerHit()
    {
        var ranking = Rank([new("a", 4, 10)], rounds: 10);

        Assert.Equal(TimeSpan.FromSeconds(2.5), ranking.Single().AverageTime);
    }

    [Fact]
    public void WithoutHitsThereIsNoAverage()
    {
        var ranking = Rank([new("a", 0)], rounds: 10);

        Assert.Null(ranking.Single().AverageTime);
    }

    [Fact]
    public void ThePercentageUsesIntegerDivision()
    {
        Player[] players = [new("a", 2)];

        var ranking = Rank(players, rounds: 3);

        // 2 * 100 / 3 = 66 (not 66.67 nor 67)
        Assert.Equal(66, ranking.Single().Percentage);
    }

    [Fact]
    public void FirstWithZeroPointsIsStillPositionOne()
    {
        Player[] players = [new("a", 0), new("b", 0)];

        var ranking = Rank(players, rounds: 5);

        Assert.Equal([1, 1], ranking.ConvertAll(r => r.Position));
        Assert.Equal([0, 0], ranking.ConvertAll(r => r.Percentage));
    }

    [Fact]
    public void WithoutParticipantsTheRankingIsEmpty()
    {
        Assert.Empty(Rank([], rounds: 5));
    }

    [Fact]
    public void ZeroOrNegativeRounds_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Rank([new("a", 1)], rounds: 0));
    }

    [Fact]
    public void TotalScore_AddsEverythingUp()
    {
        Player[] players = [new("a", 1), new("b", 5), new("c", 3)];

        Assert.Equal(9, TriviaScoring.TotalScore(players, j => j.Score));
    }
}
