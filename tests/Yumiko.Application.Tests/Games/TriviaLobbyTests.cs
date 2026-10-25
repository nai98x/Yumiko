using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaLobbyTests
{
    private const ulong Owner = 1;
    private const ulong Other = 2;
    private const ulong Third = 3;

    [Fact]
    public void TheOwnerStartsSignedUp()
    {
        TriviaLobby lobby = new(Owner);

        Assert.Equal([Owner], lobby.Players);
        Assert.Equal(TriviaLobbyStatus.Open, lobby.Status);
    }

    [Fact]
    public void Join_AddsInOrderAndOnlyOnce()
    {
        TriviaLobby lobby = new(Owner);

        Assert.Equal(TriviaLobbyJoinResult.Joined, lobby.Join(Other));
        Assert.Equal(TriviaLobbyJoinResult.Joined, lobby.Join(Third));
        Assert.Equal(TriviaLobbyJoinResult.AlreadyJoined, lobby.Join(Other));
        Assert.Equal(TriviaLobbyJoinResult.AlreadyJoined, lobby.Join(Owner));
        Assert.Equal([Owner, Other, Third], lobby.Players);
    }

    [Fact]
    public void Leave_RemovesThePlayerButNeverTheOwner()
    {
        TriviaLobby lobby = new(Owner);
        lobby.Join(Other);

        Assert.Equal(TriviaLobbyLeaveResult.IsOwner, lobby.Leave(Owner));
        Assert.Equal(TriviaLobbyLeaveResult.NotJoined, lobby.Leave(Third));
        Assert.Equal(TriviaLobbyLeaveResult.Left, lobby.Leave(Other));
        Assert.Equal([Owner], lobby.Players);
    }

    [Fact]
    public void Version_ChangesOnlyWithTheRoster()
    {
        TriviaLobby lobby = new(Owner);
        int initial = lobby.Version;

        lobby.Join(Owner);
        lobby.Leave(Third);
        Assert.Equal(initial, lobby.Version);

        lobby.Join(Other);
        Assert.NotEqual(initial, lobby.Version);
    }

    [Fact]
    public void Start_OnlyTheOwner()
    {
        TriviaLobby lobby = new(Owner);
        lobby.Join(Other);

        Assert.Equal(TriviaLobbyStartResult.NotOwner, lobby.Start(Other));
        Assert.Equal(TriviaLobbyStatus.Open, lobby.Status);
        Assert.Equal(TriviaLobbyStartResult.Started, lobby.Start(Owner));
        Assert.Equal(TriviaLobbyStatus.Started, lobby.Status);
    }

    [Fact]
    public void TheRosterIsFrozenOnceStarted()
    {
        TriviaLobby lobby = new(Owner);
        lobby.Join(Other);
        lobby.Start(Owner);

        Assert.Equal(TriviaLobbyJoinResult.NotOpen, lobby.Join(Third));
        Assert.Equal(TriviaLobbyLeaveResult.NotOpen, lobby.Leave(Other));
        Assert.Equal(TriviaLobbyStartResult.NotOpen, lobby.Start(Owner));
        Assert.Equal([Owner, Other], lobby.Players);
    }

    [Fact]
    public void Close_RejectsEverythingAfterwards()
    {
        TriviaLobby lobby = new(Owner);

        Assert.True(lobby.Close());
        Assert.Equal(TriviaLobbyStatus.Closed, lobby.Status);
        Assert.Equal(TriviaLobbyJoinResult.NotOpen, lobby.Join(Other));
        Assert.Equal(TriviaLobbyStartResult.NotOpen, lobby.Start(Owner));
    }

    [Fact]
    public void Close_LosesAgainstAnEarlierStart()
    {
        TriviaLobby lobby = new(Owner);
        lobby.Start(Owner);

        Assert.False(lobby.Close());
        Assert.Equal(TriviaLobbyStatus.Started, lobby.Status);
    }
}
