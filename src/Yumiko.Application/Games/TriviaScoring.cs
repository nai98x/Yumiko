namespace Yumiko.Application.Games;

/// <summary>Position and hit percentage of a participant in the final ranking.</summary>
public sealed record TriviaRank<T>(T Participant, int Score, int Position, int Percentage);

public static class TriviaScoring
{
    /// <summary>
    /// Sorts participants by descending score and assigns them a position. Ties share
    /// position. The percentage uses integer division.
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
