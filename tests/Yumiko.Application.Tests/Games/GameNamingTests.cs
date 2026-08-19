using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

// These strings are the Spanish labels shown in the embeds and the Firestore document ids the
// migration reads. If a test here fails, the test is not touched: the change that broke it is
// reverted.
public class GameNamingTests
{
    [Theory]
    [InlineData(Difficulty.Easy, "Fácil")]
    [InlineData(Difficulty.Normal, "Media")]
    [InlineData(Difficulty.Hard, "Dificil")]
    [InlineData(Difficulty.Extreme, "Extremo")]
    public void Difficulty_ToSpanish_KeepsTheInconsistentAccents(Difficulty difficulty, string expected)
    {
        Assert.Equal(expected, difficulty.ToSpanish());
    }

    [Theory]
    [InlineData(Gamemode.Characters, "personaje")]
    [InlineData(Gamemode.Animes, "anime")]
    [InlineData(Gamemode.Mangas, "manga")]
    [InlineData(Gamemode.Studios, "estudio")]
    [InlineData(Gamemode.Protagonists, "protagonista")]
    [InlineData(Gamemode.Genres, "genero")]
    public void Gamemode_ToSpanish_KeepsTheNames(Gamemode gamemode, string expected)
    {
        Assert.Equal(expected, gamemode.ToSpanish());
    }

    [Fact]
    public void EveryEnumValueHasAName()
    {
        foreach (Difficulty difficulty in System.Enum.GetValues<Difficulty>())
        {
            Assert.False(string.IsNullOrEmpty(difficulty.ToSpanish()));
        }

        foreach (Gamemode gamemode in System.Enum.GetValues<Gamemode>())
        {
            Assert.False(string.IsNullOrEmpty(gamemode.ToSpanish()));
        }
    }

    [Fact]
    public void ValueOutsideTheEnum_Throws()
    {
        // If someone adds a value to the enum without giving it a name, it has to break here instead
        // of silently showing a label that is not there.
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Difficulty)99).ToSpanish());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Gamemode)99).ToSpanish());
    }

    [Fact]
    public void FromSpanish_RoundTripsEveryEnumValue()
    {
        foreach (Difficulty difficulty in System.Enum.GetValues<Difficulty>())
        {
            Assert.Equal(difficulty, GameNaming.DifficultyFromSpanish(difficulty.ToSpanish()));
        }

        foreach (Gamemode gamemode in System.Enum.GetValues<Gamemode>())
        {
            Assert.Equal(gamemode, GameNaming.GamemodeFromSpanish(gamemode.ToSpanish()));
        }
    }

    [Theory]
    [InlineData("Facil")]
    [InlineData("Difícil")]
    [InlineData("Easy")]
    [InlineData("")]
    public void DifficultyFromSpanish_UnknownName_IsNull(string name)
    {
        // The migration counts these as skipped instead of guessing a difficulty.
        Assert.Null(GameNaming.DifficultyFromSpanish(name));
    }

    [Theory]
    [InlineData("Personaje")]
    [InlineData("géneros")]
    [InlineData("characters")]
    public void GamemodeFromSpanish_UnknownName_IsNull(string name)
    {
        Assert.Null(GameNaming.GamemodeFromSpanish(name));
    }
}
