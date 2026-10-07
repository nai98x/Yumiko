using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class HigherOrLowerDuoStateTests
{
    [Fact]
    public void TheFirstGivenPlayerStarts()
    {
        var state = new HigherOrLowerDuoState(1, 2);

        Assert.Equal(1UL, state.CurrentPlayer);
        Assert.Equal(0, state.Score);
        Assert.False(state.IsFinished);
    }

    [Fact]
    public void HitsAlternateTheTurnAndAddToTheSharedScore()
    {
        var state = new HigherOrLowerDuoState(1, 2);

        state.Answer(true, TimeSpan.FromSeconds(1));
        Assert.Equal(2UL, state.CurrentPlayer);

        state.Answer(true, TimeSpan.FromSeconds(1));
        Assert.Equal(1UL, state.CurrentPlayer);

        state.Answer(true, TimeSpan.FromSeconds(1));
        Assert.Equal(2UL, state.CurrentPlayer);
        Assert.Equal(3, state.Score);
    }

    [Fact]
    public void AMissEndsTheMatchAndRemembersWhoMissed()
    {
        var state = new HigherOrLowerDuoState(1, 2);

        state.Answer(true, TimeSpan.FromSeconds(1));
        state.Answer(false, TimeSpan.FromSeconds(1));

        Assert.True(state.IsFinished);
        Assert.Equal(2UL, state.MissedBy);
        Assert.Equal(1, state.Score);
        Assert.Throws<InvalidOperationException>(() => state.Answer(true, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void SummariesSplitHitsAndTimesByPlayerCountingTheMiss()
    {
        var state = new HigherOrLowerDuoState(1, 2);

        state.Answer(true, TimeSpan.FromSeconds(2));
        state.Answer(true, TimeSpan.FromSeconds(5));
        state.Answer(true, TimeSpan.FromSeconds(4));
        state.Answer(false, TimeSpan.FromSeconds(9));

        IReadOnlyList<HigherOrLowerDuoPlayerSummary> summaries = state.Summaries();

        Assert.Equal(new HigherOrLowerDuoPlayerSummary(1, 2, 2, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2)), summaries[0]);
        Assert.Equal(new HigherOrLowerDuoPlayerSummary(2, 1, 2, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(5)), summaries[1]);
    }

    [Fact]
    public void APlayerWithoutAnswersHasNoTimes()
    {
        var state = new HigherOrLowerDuoState(1, 2);

        state.Answer(false, TimeSpan.FromSeconds(3));

        HigherOrLowerDuoPlayerSummary second = state.Summaries()[1];

        Assert.Equal(0, second.Answers);
        Assert.Null(second.AverageTime);
        Assert.Null(second.FastestTime);
    }

    [Fact]
    public void ThePlayersMustBeDifferent()
    {
        Assert.Throws<ArgumentException>(() => new HigherOrLowerDuoState(1, 1));
    }
}
