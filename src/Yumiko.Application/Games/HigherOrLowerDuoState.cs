namespace Yumiko.Application.Games;

public sealed record HigherOrLowerDuoPlayerSummary(ulong UserId, int Hits, int Answers, TimeSpan? AverageTime, TimeSpan? FastestTime);

/// <summary>
/// State of a Higher or Lower duo match: the two players alternate turns and share the score, and the
/// first miss ends the match. It knows nothing about Discord nor about the media being compared.
/// </summary>
public sealed class HigherOrLowerDuoState
{
    private readonly ulong[] _players;
    private readonly List<TimeSpan>[] _times = [[], []];
    private readonly int[] _hits = new int[2];
    private int _turn;

    public HigherOrLowerDuoState(ulong firstPlayer, ulong secondPlayer)
    {
        if (firstPlayer == secondPlayer)
        {
            throw new ArgumentException("The two players must be different users.", nameof(secondPlayer));
        }

        _players = [firstPlayer, secondPlayer];
    }

    /// <summary>The player who has to answer the current round.</summary>
    public ulong CurrentPlayer => _players[_turn];

    public int Score => _hits.Sum();

    /// <summary>The player who missed, or <c>null</c> while nobody has missed.</summary>
    public ulong? MissedBy { get; private set; }

    public bool IsFinished => MissedBy is not null;

    /// <summary>
    /// Records the answer of <see cref="CurrentPlayer"/>. A hit adds to the shared score and passes the turn;
    /// a miss ends the match. Every answer counts towards the response times, the miss included.
    /// </summary>
    public void Answer(bool isCorrect, TimeSpan elapsed)
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("The match is already finished.");
        }

        _times[_turn].Add(elapsed);

        if (!isCorrect)
        {
            MissedBy = CurrentPlayer;
            return;
        }

        _hits[_turn]++;
        _turn = 1 - _turn;
    }

    /// <summary>Summaries in the order the players were given.</summary>
    public IReadOnlyList<HigherOrLowerDuoPlayerSummary> Summaries() => [Summary(0), Summary(1)];

    private HigherOrLowerDuoPlayerSummary Summary(int index)
    {
        List<TimeSpan> times = _times[index];

        return new HigherOrLowerDuoPlayerSummary(
            _players[index],
            _hits[index],
            times.Count,
            times.Count == 0 ? null : TimeSpan.FromTicks((long)times.Average(t => t.Ticks)),
            times.Count == 0 ? null : times.Min());
    }
}
