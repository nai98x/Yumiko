using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class HangmanStateTests
{
    [Fact]
    public void StartsWithEveryLetterHidden()
    {
        var state = new HangmanState("naruto");

        Assert.Equal("_ _ _ _ _ _", state.Masked());
        Assert.Equal(0, state.Mistakes);
        Assert.False(state.IsFinished);
    }

    [Fact]
    public void NonLettersAreShownFromTheStart()
    {
        var state = new HangmanState("Re:Zero");

        Assert.Equal("_ _ : _ _ _ _", state.Masked());
    }

    [Fact]
    public void RepeatedSpacesAreCollapsed()
    {
        var state = new HangmanState("  cowboy    bebop  ");

        Assert.Equal("_ _ _ _ _ _   _ _ _ _ _", state.Masked());
    }

    [Fact]
    public void AHitRevealsEveryOccurrence()
    {
        var state = new HangmanState("naruto");

        Assert.True(state.Guess("n"));

        Assert.Equal("n _ _ _ _ _", state.Masked());
        Assert.Equal(0, state.Mistakes);
    }

    [Fact]
    public void AMissAddsOneMistake()
    {
        var state = new HangmanState("naruto");

        Assert.False(state.Guess("z"));

        Assert.Equal(1, state.Mistakes);
        Assert.Equal("_ _ _ _ _ _", state.Masked());
    }

    [Fact]
    public void SixMistakesLoseTheGame()
    {
        var state = new HangmanState("naruto");

        foreach (string letter in new[] { "z", "x", "q", "w", "k", "j" })
        {
            state.Guess(letter);
        }

        Assert.True(state.IsLost);
        Assert.True(state.IsFinished);
        Assert.False(state.IsComplete);
    }

    [Fact]
    public void GuessingEveryLetterWinsTheGame()
    {
        var state = new HangmanState("naruto");

        foreach (string letter in new[] { "n", "a", "r", "u", "t", "o" })
        {
            Assert.True(state.Guess(letter));
        }

        Assert.True(state.IsComplete);
        Assert.True(state.IsFinished);
        Assert.False(state.IsLost);
        Assert.Equal("n a r u t o", state.Masked());
    }

    [Fact]
    public void UsedLettersAreNotRepeated()
    {
        var state = new HangmanState("naruto");

        state.Guess("n");
        state.Guess("N");
        state.Guess(" n ");
        state.Guess("z");

        Assert.Equal(["n", "z"], state.UsedLetters);
    }

    [Fact]
    public void RetryingAWrongLetterAddsAnotherMistake()
    {
        var state = new HangmanState("naruto");

        state.Guess("z");
        state.Guess("z");

        Assert.Equal(2, state.Mistakes);
    }

    [Fact]
    public void RevealAll_ReturnsHowManyLettersWereMissing()
    {
        var state = new HangmanState("naruto");
        state.Guess("n");

        int remaining = state.RevealAll();

        Assert.Equal(5, remaining);
        Assert.True(state.IsComplete);
        Assert.Equal("n a r u t o", state.Masked());
    }

    [Fact]
    public void Surrender_MarksTheGameAsLost()
    {
        var state = new HangmanState("naruto");

        state.Surrender();

        Assert.True(state.IsLost);
        Assert.Equal(HangmanState.MaxMistakes, state.Mistakes);
    }

    [Fact]
    public void AddMistake_DoesNotRecordAUsedLetter()
    {
        var state = new HangmanState("naruto");

        state.AddMistake();

        Assert.Equal(1, state.Mistakes);
        Assert.Empty(state.UsedLetters);
    }

    [Fact]
    public void AnEmptyWordIsNotValid()
    {
        Assert.Throws<ArgumentException>(() => new HangmanState("   "));
    }

    [Fact]
    public void Word_ExposesTheNormalizedTextForThePresentationToFormat()
    {
        HangmanState state = new("  Cowboy   Bebop  ");

        // It is lowercased, trimmed and repeated spaces are collapsed into one.
        Assert.Equal("cowboy bebop", new string([.. state.Word]));
    }

    [Fact]
    public void IsRevealed_StartsTrueOnlyForNonLetters()
    {
        HangmanState state = new("re:zero");

        Assert.True(state.IsRevealed(':'));
        Assert.False(state.IsRevealed('z'));

        state.Guess("z");

        Assert.True(state.IsRevealed('z'));
    }
}
