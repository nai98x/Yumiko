using Yumiko.Model.Entities;

namespace Yumiko.Application.Games;

public sealed record Rank<T>(T Player, int Position);

public static class LeaderboardRanking
{
    /// <summary>
    /// Assigns positions to the trivia leaderboard. Two players share a position only if they match
    /// in hit percentage <em>and</em> in games played.
    /// </summary>
    /// <remarks>
    /// Players averaging less than 2 rounds per game are discarded: those are abandoned
    /// games that would inflate the percentage. The division is integer.
    /// </remarks>
    public static List<Rank<GameStats>> RankQuiz(IEnumerable<GameStats> players)
    {
        List<Rank<GameStats>> ranks = [];
        int position = 0;
        int lastPercentage = -1;
        int lastGamesPlayed = -1;

        foreach (GameStats player in players)
        {
            if (player.GamesPlayed == 0 || player.TotalRounds / player.GamesPlayed < 2)
            {
                continue;
            }

            if (player.AccuracyPercentage != lastPercentage || player.GamesPlayed != lastGamesPlayed)
            {
                position++;
            }

            ranks.Add(new Rank<GameStats>(player, position));
            lastPercentage = player.AccuracyPercentage;
            lastGamesPlayed = player.GamesPlayed;
        }

        return ranks;
    }

    /// <summary>Positions of the Higher or Lower leaderboard, tying by score.</summary>
    public static List<Rank<HigherOrLowerEntry>> RankHigherOrLower(IEnumerable<HigherOrLowerEntry> players, int maxRanks = 10) =>
        RankByScore(players, p => p.Score, maxRanks);

    /// <summary>Positions of the Higher or Lower duo leaderboard, tying by score.</summary>
    public static List<Rank<HigherOrLowerDuoEntry>> RankHigherOrLowerDuo(IEnumerable<HigherOrLowerDuoEntry> pairs, int maxRanks = 10) =>
        RankByScore(pairs, p => p.Score, maxRanks);

    private static List<Rank<T>> RankByScore<T>(IEnumerable<T> players, Func<T, int> score, int maxRanks)
    {
        List<Rank<T>> ranks = [];
        int position = 0;
        int lastScore = -1;

        foreach (T player in players)
        {
            if (score(player) != lastScore)
            {
                position++;
            }

            if (position > maxRanks)
            {
                break;
            }

            ranks.Add(new Rank<T>(player, position));
            lastScore = score(player);
        }

        return ranks;
    }
}
