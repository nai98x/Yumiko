using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

// These names are a production contract: they are the document ids everything is already stored with
// in Firestore. The accents are inconsistent on purpose ("Fácil" with accent, "Dificil" without): changing them
// orphans the existing data.
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
