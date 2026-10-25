namespace Yumiko.Application.Games;

/// <summary>
/// Position, hit percentage and average time per hit of a participant in the final ranking. The average is
/// null when there are no hits.
/// </summary>
public sealed record TriviaRank<T>(T Participant, int Score, int Position, int Percentage, TimeSpan? AverageTime);

public static class TriviaScoring
{
    /// <summary>
    /// Sorts participants by descending score and assigns them a position; a tie in score goes to the lowest
    /// average time per hit. Only a tie in both shares position. The percentage uses integer division.
    /// </summary>
    /// <param name="totalTime">Time spent on the hits only, added up.</param>
    public static List<TriviaRank<T>> Rank<T>(
        IEnumerable<T> participants,
        Func<T, int> score,
        Func<T, TimeSpan> totalTime,
        int rounds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rounds);

        List<TriviaRank<T>> ranking = [];
        int position = 0;
        (int Score, TimeSpan? Average)? previous = null;

        IEnumerable<(T Participant, int Score, TimeSpan? Average)> sorted = participants
            .Select(p => (Participant: p, Score: score(p), Average: AverageTime(totalTime(p), score(p))))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Average ?? TimeSpan.MaxValue);

        foreach ((T participant, int points, TimeSpan? average) in sorted)
        {
            if (previous != (points, average))
            {
                position++;
            }

            ranking.Add(new TriviaRank<T>(participant, points, position, points * 100 / rounds, average));
            previous = (points, average);
        }

        return ranking;
    }

    public static TimeSpan? AverageTime(TimeSpan totalTime, int score) =>
        score > 0 ? totalTime / score : null;

    public static int TotalScore<T>(IEnumerable<T> participants, Func<T, int> score) =>
        participants.Sum(score);
}
