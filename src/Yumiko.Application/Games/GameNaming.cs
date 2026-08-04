using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

// Estos nombres son contrato de producción: son los ids de documento con los que ya está guardado todo
// en Firestore. Las tildes son inconsistentes a propósito ("Fácil" con tilde, "Dificil" sin): cambiarlas
// huerfaniza los datos existentes.
public static class GameNaming
{
    public static string ToSpanish(this Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "Fácil",
        Difficulty.Normal => "Media",
        Difficulty.Hard => "Dificil",
        Difficulty.Extreme => "Extremo",
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty)),
    };

    public static string ToSpanish(this Gamemode gamemode) => gamemode switch
    {
        Gamemode.Characters => "personaje",
        Gamemode.Animes => "anime",
        Gamemode.Mangas => "manga",
        Gamemode.Studios => "estudio",
        Gamemode.Protagonists => "protagonista",
        Gamemode.Genres => "genero",
        _ => throw new ArgumentOutOfRangeException(nameof(gamemode)),
    };
}
