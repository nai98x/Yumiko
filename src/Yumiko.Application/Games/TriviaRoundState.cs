using Yumiko.Model.Enum;

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
/// Who can still answer one trivia round. Every player gets a single attempt per round; in
/// <see cref="TriviaPlayers.Singleplayer"/> only the owner plays and a miss ends the round.
/// Attempts arrive concurrently from gateway threads, so only the first correct one wins.
/// </summary>
public sealed class TriviaRoundState
{
    private readonly Lock _lock = new();
    private readonly HashSet<ulong> _attempted = [];
    private readonly HashSet<string> _options;
    private readonly string _correctOption;
    private readonly TriviaPlayers _players;
    private readonly ulong _ownerId;

    public TriviaRoundState(IEnumerable<string> options, string correctOption, TriviaPlayers players, ulong ownerId)
    {
        _options = [.. options];

        if (!_options.Contains(correctOption))
        {
            throw new ArgumentException("The correct option must be one of the options.", nameof(correctOption));
        }

        _correctOption = correctOption;
        _players = players;
        _ownerId = ownerId;
    }

    public bool IsFinished { get; private set; }

    public ulong? WinnerId { get; private set; }

    /// <summary>Option ids not belonging to this round come from the buttons of a past round.</summary>
    public TriviaAttemptResult Attempt(ulong userId, string option)
    {
        lock (_lock)
        {
            if (_players == TriviaPlayers.Singleplayer && userId != _ownerId)
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

            if (_players == TriviaPlayers.Singleplayer)
            {
                IsFinished = true;
            }

            return TriviaAttemptResult.Wrong;
        }
    }
}
