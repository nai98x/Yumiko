namespace Yumiko.Application.Games;

/// <summary>Posición y porcentaje de aciertos de un participante en el ranking final.</summary>
public sealed record TriviaRank<T>(T Participant, int Score, int Position, int Percentage);

public static class TriviaScoring
{
    /// <summary>
    /// Ordena los participantes por puntaje descendente y les asigna posición. Los que empatan comparten
    /// posición. El porcentaje usa división entera.
    /// </summary>
    public static List<TriviaRank<T>> Rank<T>(IEnumerable<T> participants, Func<T, int> score, int rounds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rounds);

        List<TriviaRank<T>> ranking = [];
        int position = 0;
        int? previousScore = null;

        foreach (T participant in participants.OrderByDescending(score))
        {
            int points = score(participant);

            if (previousScore != points)
            {
                position++;
            }

            ranking.Add(new TriviaRank<T>(participant, points, position, points * 100 / rounds));
            previousScore = points;
        }

        return ranking;
    }

    public static int TotalScore<T>(IEnumerable<T> participants, Func<T, int> score) =>
        participants.Sum(score);
}
