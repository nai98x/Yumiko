using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

// Estos strings son ids de documento de Firestore en producción. Si un test de acá falla, no se toca
// el test: se revierte el cambio que lo rompió.
public class GameNamingTests
{
    [Theory]
    [InlineData(Difficulty.Easy, "Fácil")]
    [InlineData(Difficulty.Normal, "Media")]
    [InlineData(Difficulty.Hard, "Dificil")]
    [InlineData(Difficulty.Extreme, "Extremo")]
    public void Difficulty_ToSpanish_KeepsTheInconsistentProductionAccents(Difficulty difficulty, string expected)
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
    public void Gamemode_ToSpanish_KeepsTheProductionNames(Gamemode gamemode, string expected)
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
        // Si alguien agrega un valor al enum sin darle nombre, tiene que romper acá y no guardar
        // silenciosamente un documento con otro id en Firestore.
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Difficulty)99).ToSpanish());
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Gamemode)99).ToSpanish());
    }
}
