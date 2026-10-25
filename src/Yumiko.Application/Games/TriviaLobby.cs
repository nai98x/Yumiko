namespace Yumiko.Application.Games;

public enum TriviaLobbyStatus
{
    Open,
    Started,
    Closed,
}

public enum TriviaLobbyJoinResult
{
    Joined,
    AlreadyJoined,
    NotOpen,
}

public enum TriviaLobbyLeaveResult
{
    Left,
    NotJoined,
    IsOwner,
    NotOpen,
}

public enum TriviaLobbyStartResult
{
    Started,
    NotOwner,
    NotOpen,
}

/// <summary>
/// Sign-up phase of a multiplayer trivia: players join until the owner starts the match, and only they
/// can answer afterwards. The owner is always in and cannot leave. Clicks arrive concurrently from
/// gateway threads.
/// </summary>
public sealed class TriviaLobby
{
    private readonly Lock _lock = new();
    private readonly List<ulong> _players;

    public TriviaLobby(ulong ownerId)
    {
        OwnerId = ownerId;
        _players = [ownerId];
    }

    public ulong OwnerId { get; }

    public TriviaLobbyStatus Status { get; private set; }

    /// <summary>Bumped on every roster change, so whoever shows the lobby knows when to redraw it.</summary>
    public int Version { get; private set; }

    public IReadOnlyList<ulong> Players
    {
        get
        {
            lock (_lock)
            {
                return [.. _players];
            }
        }
    }

    public TriviaLobbyJoinResult Join(ulong userId)
    {
        lock (_lock)
        {
            if (Status != TriviaLobbyStatus.Open)
            {
                return TriviaLobbyJoinResult.NotOpen;
            }

            if (_players.Contains(userId))
            {
                return TriviaLobbyJoinResult.AlreadyJoined;
            }

            _players.Add(userId);
            Version++;
            return TriviaLobbyJoinResult.Joined;
        }
    }

    public TriviaLobbyLeaveResult Leave(ulong userId)
    {
        lock (_lock)
        {
            if (Status != TriviaLobbyStatus.Open)
            {
                return TriviaLobbyLeaveResult.NotOpen;
            }

            if (userId == OwnerId)
            {
                return TriviaLobbyLeaveResult.IsOwner;
            }

            if (!_players.Remove(userId))
            {
                return TriviaLobbyLeaveResult.NotJoined;
            }

            Version++;
            return TriviaLobbyLeaveResult.Left;
        }
    }

    public TriviaLobbyStartResult Start(ulong userId)
    {
        lock (_lock)
        {
            if (Status != TriviaLobbyStatus.Open)
            {
                return TriviaLobbyStartResult.NotOpen;
            }

            if (userId != OwnerId)
            {
                return TriviaLobbyStartResult.NotOwner;
            }

            Status = TriviaLobbyStatus.Started;
            return TriviaLobbyStartResult.Started;
        }
    }

    /// <summary>Returns false when the owner started the match first.</summary>
    public bool Close()
    {
        lock (_lock)
        {
            if (Status == TriviaLobbyStatus.Started)
            {
                return false;
            }

            Status = TriviaLobbyStatus.Closed;
            return true;
        }
    }
}
