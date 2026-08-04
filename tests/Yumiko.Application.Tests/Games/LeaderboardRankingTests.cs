using Yumiko.Application.Games;
using Yumiko.Model.Entities;

namespace Yumiko.Application.Tests.Games;

public class LeaderboardRankingTests
{
    private static GameStats Player(ulong id, int percentage, int gamesPlayed, int rounds) => new()
    {
        UserId = (long)id,
        AccuracyPercentage = percentage,
        GamesPlayed = gamesPlayed,
        TotalRounds = rounds,
    };

    [Fact]
    public void RankQuiz_DropsPlayersWithFewerThanTwoRoundsPerGame()
    {
        List<GameStats> players =
        [
            Player(1, 100, 5, 5),   // 1 ronda por partida: fuera
            Player(2, 80, 5, 20),   // 4 rondas por partida: entra
        ];

        List<Rank<GameStats>> ranks = LeaderboardRanking.RankQuiz(players);

        Assert.Single(ranks);
        Assert.Equal(2, ranks[0].Player.UserId);
        Assert.Equal(1, ranks[0].Position);
    }

    [Fact]
    public void RankQuiz_TiesOnlyWhenPercentageAndGamesMatch()
    {
        List<GameStats> players =
        [
            Player(1, 90, 4, 40),
            Player(2, 90, 4, 40),  // empate real
            Player(3, 90, 2, 20),  // mismo porcentaje, menos partidas: puesto propio
            Player(4, 50, 2, 20),
        ];

        List<Rank<GameStats>> ranks = LeaderboardRanking.RankQuiz(players);

        Assert.Equal([1, 1, 2, 3], ranks.Select(p => p.Position));
    }

    [Fact]
    public void RankQuiz_StartsAtOneEvenIfTheFirstHasZeroPercent()
    {
        List<Rank<GameStats>> ranks = LeaderboardRanking.RankQuiz([Player(1, 0, 3, 30)]);

        Assert.Equal(1, ranks[0].Position);
    }

    [Fact]
    public void RankQuiz_IgnoresPlayersWithoutGames()
    {
        Assert.Empty(LeaderboardRanking.RankQuiz([Player(1, 100, 0, 0)]));
    }

    [Fact]
    public void RankHigherOrLower_TiesOnScoreAndStopsAtTheMaximum()
    {
        List<HigherOrLowerEntry> players =
        [
            new() { UserId = 1, Score = 50 },
            new() { UserId = 2, Score = 50 },
            new() { UserId = 3, Score = 40 },
            new() { UserId = 4, Score = 30 },
        ];

        List<Rank<HigherOrLowerEntry>> ranks = LeaderboardRanking.RankHigherOrLower(players, maxRanks: 2);

        Assert.Equal([1, 1, 2], ranks.Select(p => p.Position));
    }

    [Fact]
    public void RankHigherOrLower_StartsAtOneEvenIfTheFirstScoreIsZero()
    {
        List<Rank<HigherOrLowerEntry>> ranks =
            LeaderboardRanking.RankHigherOrLower([new HigherOrLowerEntry { UserId = 1, Score = 0 }]);

        Assert.Equal(1, ranks[0].Position);
    }
}
