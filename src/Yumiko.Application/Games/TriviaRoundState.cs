namespace Yumiko.Application.Games;

public enum TriviaAttemptResult
{
    Correct,
    Wrong,
    NotAllowed,
    AlreadyTried,
    RoundOver,
}

/// <summary>
/// Who can still answer one trivia round. Only the given players can answer, each with a single attempt;
/// the round ends on the first correct attempt or once every player has missed.
/// Attempts arrive concurrently from gateway threads, so only the first correct one wins.
/// </summary>
public sealed class TriviaRoundState
{
    private readonly Lock _lock = new();
    private readonly HashSet<ulong> _attempted = [];
    private readonly HashSet<string> _options;
    private readonly string _correctOption;
    private readonly HashSet<ulong> _players;

    public TriviaRoundState(IEnumerable<string> options, string correctOption, IEnumerable<ulong> players)
    {
        _options = [.. options];

        if (!_options.Contains(correctOption))
        {
            throw new ArgumentException("The correct option must be one of the options.", nameof(correctOption));
        }

        _correctOption = correctOption;
        _players = [.. players];
    }

    public bool IsFinished { get; private set; }

    public ulong? WinnerId { get; private set; }

    /// <summary>Option ids not belonging to this round come from the buttons of a past round.</summary>
    public TriviaAttemptResult Attempt(ulong userId, string option)
    {
        lock (_lock)
        {
            if (!_players.Contains(userId))
            {
                return TriviaAttemptResult.NotAllowed;
            }

            if (IsFinished || !_options.Contains(option))
            {
                return TriviaAttemptResult.RoundOver;
            }

            if (!_attempted.Add(userId))
            {
                return TriviaAttemptResult.AlreadyTried;
            }

            if (option == _correctOption)
            {
                WinnerId = userId;
                IsFinished = true;
                return TriviaAttemptResult.Correct;
            }

            if (_attempted.IsSupersetOf(_players))
            {
                IsFinished = true;
            }

            return TriviaAttemptResult.Wrong;
        }
    }
}
