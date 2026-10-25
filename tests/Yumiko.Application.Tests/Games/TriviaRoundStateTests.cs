using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaRoundStateTests
{
    private const ulong Owner = 1;
    private const ulong Other = 2;
    private const ulong Third = 3;
    private const ulong Outsider = 4;

    private static readonly string[] Options = ["a", "b", "c", "d", "e"];

    private static TriviaRoundState Solo() => new(Options, "a", [Owner]);

    private static TriviaRoundState Group() => new(Options, "a", [Owner, Other, Third]);

    [Fact]
    public void Constructor_RejectsACorrectOptionOutsideTheOptions()
    {
        Assert.Throws<ArgumentException>(() => new TriviaRoundState(Options, "z", [Owner]));
    }

    [Fact]
    public void Solo_OnlyTheOwnerCanAnswer()
    {
        TriviaRoundState state = Solo();

        Assert.Equal(TriviaAttemptResult.NotAllowed, state.Attempt(Other, "a"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Owner, "a"));
        Assert.Equal(Owner, state.WinnerId);
    }

    [Fact]
    public void Solo_AMissEndsTheRound()
    {
        TriviaRoundState state = Solo();

        Assert.Equal(TriviaAttemptResult.Wrong, state.Attempt(Owner, "b"));
        Assert.True(state.IsFinished);
        Assert.Null(state.WinnerId);
        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Owner, "a"));
    }

    [Fact]
    public void Solo_OthersAreRejectedEvenAfterTheRoundEnds()
    {
        TriviaRoundState state = Solo();
        state.Attempt(Owner, "b");

        Assert.Equal(TriviaAttemptResult.NotAllowed, state.Attempt(Other, "a"));
    }

    [Fact]
    public void Group_AnyPlayerCanAnswer()
    {
        TriviaRoundState state = Group();

        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Other, "a"));
        Assert.Equal(Other, state.WinnerId);
        Assert.True(state.IsFinished);
    }

    [Fact]
    public void Group_OutsidersCannotAnswer()
    {
        TriviaRoundState state = Group();

        Assert.Equal(TriviaAttemptResult.NotAllowed, state.Attempt(Outsider, "a"));
        Assert.False(state.IsFinished);
    }

    [Fact]
    public void Group_AMissLocksOnlyThatPlayer()
    {
        TriviaRoundState state = Group();

        Assert.Equal(TriviaAttemptResult.Wrong, state.Attempt(Other, "b"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.AlreadyTried, state.Attempt(Other, "a"));
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Third, "a"));
        Assert.Equal(Third, state.WinnerId);
    }

    [Fact]
    public void Group_EndsOnceEveryPlayerHasMissed()
    {
        TriviaRoundState state = Group();

        state.Attempt(Owner, "b");
        state.Attempt(Other, "c");
        Assert.False(state.IsFinished);

        Assert.Equal(TriviaAttemptResult.Wrong, state.Attempt(Third, "d"));
        Assert.True(state.IsFinished);
        Assert.Null(state.WinnerId);
    }

    [Fact]
    public void Group_OnlyTheFirstCorrectAttemptWins()
    {
        TriviaRoundState state = Group();

        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Other, "a"));
        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Third, "a"));
        Assert.Equal(Other, state.WinnerId);
    }

    [Fact]
    public void Group_ConcurrentCorrectAttemptsHaveASingleWinner()
    {
        ulong[] players = [.. Enumerable.Range(10, 100).Select(i => (ulong)i)];
        TriviaRoundState state = new(Options, "a", players);

        TriviaAttemptResult[] results = new TriviaAttemptResult[players.Length];
        Parallel.For(0, results.Length, i => results[i] = state.Attempt(players[i], "a"));

        Assert.Single(results, r => r == TriviaAttemptResult.Correct);
        Assert.All(results.Where(r => r != TriviaAttemptResult.Correct), r => Assert.Equal(TriviaAttemptResult.RoundOver, r));
    }

    [Fact]
    public void OptionsFromAnotherRoundAreTreatedAsOver()
    {
        TriviaRoundState state = Group();

        Assert.Equal(TriviaAttemptResult.RoundOver, state.Attempt(Owner, "old-round-option"));
        Assert.False(state.IsFinished);
        Assert.Equal(TriviaAttemptResult.Correct, state.Attempt(Owner, "a"));
    }
}
