using Yumiko.Model.Entities;

namespace Yumiko.Application.Games;

public sealed record Rank<T>(T Player, int Position);

public static class LeaderboardRanking
{
    /// <summary>
    /// Asigna posiciones al leaderboard de trivia. Dos jugadores comparten posición solo si coinciden
    /// en porcentaje de aciertos <em>y</em> en partidas jugadas.
    /// </summary>
    /// <remarks>
    /// Se descartan los jugadores con menos de 2 rondas por partida en promedio: son partidas
    /// abandonadas que inflarían el porcentaje. La división es entera.
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

    /// <summary>Posiciones del leaderboard de Higher or Lower, empatando por puntuación.</summary>
    public static List<Rank<HigherOrLowerEntry>> RankHigherOrLower(IEnumerable<HigherOrLowerEntry> players, int maxRanks = 10)
    {
        List<Rank<HigherOrLowerEntry>> ranks = [];
        int position = 0;
        int lastScore = -1;

        foreach (HigherOrLowerEntry player in players)
        {
            if (player.Score != lastScore)
            {
                position++;
            }

            if (position > maxRanks)
            {
                break;
            }

            ranks.Add(new Rank<HigherOrLowerEntry>(player, position));
            lastScore = player.Score;
        }

        return ranks;
    }
}
