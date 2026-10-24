using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

public class TriviaRoundStateTests
{
    private const ulong Owner = 1;
    private const ulong Other = 2;
    private const ulong Third = 3;

    private static readonly string[] Options = ["a", "b", "c", "d", "e"];

    private static TriviaRoundState Create(TriviaPlayers players) => new(Options, "a", players, Owner);

    [Fact]
    public void Constructor_RejectsACorrectOptionOutsideTheOptions()
    {
        Assert.Throws<ArgumentException>(() => new TriviaRoundState(Options, "z", TriviaPlayers.Multiplayer, Owner));
    }

    [Fact]
    public void Singleplayer_OnlyTheOwnerCanAnswer()
    {
        TriviaRoundState state = Create(TriviaPlayers.Singleplayer);

        Assert.Equal(TriviaAttemptResult.NotAllowed, state.Attempt(Other, "a"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Owner, "a"));
        Assert.Equal(Owner, state.WinnerId);
    }

    [Fact]
    public void Singleplayer_AMissEndsTheRound()
    {
        TriviaRoundState state = Create(TriviaPlayers.Singleplayer);

        Assert.Equal(TriviaAttemptResult.Wrong, state.Attempt(Owner, "b"));
        Assert.True(state.IsFinished);
        Assert.Null(state.WinnerId);
        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Owner, "a"));
    }

    [Fact]
    public void Singleplayer_OthersAreRejectedEvenAfterTheRoundEnds()
    {
        TriviaRoundState state = Create(TriviaPlayers.Singleplayer);
        state.Attempt(Owner, "b");

        Assert.Equal(TriviaAttemptResult.NotAllowed, state.Attempt(Other, "a"));
    }

    [Fact]
    public void Multiplayer_AnyoneCanAnswer()
    {
        TriviaRoundState state = Create(TriviaPlayers.Multiplayer);

        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Other, "a"));
        Assert.Equal(Other, state.WinnerId);
        Assert.True(state.IsFinished);
    }

    [Fact]
    public void Multiplayer_AMissLocksOnlyThatPlayer()
    {
        TriviaRoundState state = Create(TriviaPlayers.Multiplayer);

        Assert.Equal(TriviaAttemptResult.Wrong, state.Attempt(Other, "b"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.AlreadyTried, state.Attempt(Other, "a"));
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Third, "a"));
        Assert.Equal(Third, state.WinnerId);
    }

    [Fact]
    public void Multiplayer_OnlyTheFirstCorrectAttemptWins()
    {
        TriviaRoundState state = Create(TriviaPlayers.Multiplayer);

        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Other, "a"));
        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Third, "a"));
        Assert.Equal(Other, state.WinnerId);
    }

    [Fact]
    public void Multiplayer_ConcurrentCorrectAttemptsHaveASingleWinner()
    {
        TriviaRoundState state = Create(TriviaPlayers.Multiplayer);

        TriviaAttemptResult[] results = new TriviaAttemptResult[100];
        Parallel.For(0, results.Length, i => results[i] = state.Attempt((ulong)i + 10, "a"));

        Assert.Single(results, r => r == TriviaAttemptResult.Correct);
        Assert.All(results.Where(r => r != TriviaAttemptResult.Correct), r => Assert.Equal(TriviaAttemptResult.RoundOver, r));
    }

    [Theory]
    [InlineData(TriviaPlayers.Singleplayer)]
    [InlineData(TriviaPlayers.Multiplayer)]
    public void OptionsFromAnotherRoundAreTreatedAsOver(TriviaPlayers players)
    {
        TriviaRoundState state = Create(players);

        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Owner, "old-round-option"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Owner, "a"));
    }
}
