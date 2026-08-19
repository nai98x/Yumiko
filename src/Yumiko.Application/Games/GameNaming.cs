using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

// Spanish labels of the gamemodes and difficulties: what the embeds show when the interaction locale
// is Spanish, and also the Firestore document ids the migration reads. The accents are inconsistent
// on purpose ("Fácil" with accent, "Dificil" without): that is how the documents are named.
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

    public static Difficulty? DifficultyFromSpanish(string name) => name switch
    {
        "Fácil" => Difficulty.Easy,
        "Media" => Difficulty.Normal,
        "Dificil" => Difficulty.Hard,
        "Extremo" => Difficulty.Extreme,
        _ => null,
    };

    public static Gamemode? GamemodeFromSpanish(string name) => name switch
    {
        "personaje" => Gamemode.Characters,
        "anime" => Gamemode.Animes,
        "manga" => Gamemode.Mangas,
        "estudio" => Gamemode.Studios,
        "protagonista" => Gamemode.Protagonists,
        "genero" => Gamemode.Genres,
        _ => null,
    };
}
