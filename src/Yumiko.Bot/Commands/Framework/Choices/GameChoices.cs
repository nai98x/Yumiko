using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Commands.Framework.Choices;

// Espejo de los enums de dominio con las etiquetas que muestra Discord. Los enums de Yumiko.Model no
// pueden llevar atributos de DSharpPlus porque esa capa no referencia paquetes.
public enum GamemodeChoice
{
    [ChoiceDisplayName("Characters")]
    Characters,
    [ChoiceDisplayName("Animes")]
    Animes,
    [ChoiceDisplayName("Mangas")]
    Mangas,
    [ChoiceDisplayName("Protagonists")]
    Protagonists,
    [ChoiceDisplayName("Genres")]
    Genres,
    [ChoiceDisplayName("Studios")]
    Studios,
}

public enum DifficultyChoice
{
    [ChoiceDisplayName("Easy")]
    Easy,
    [ChoiceDisplayName("Normal")]
    Normal,
    [ChoiceDisplayName("Hard")]
    Hard,
    [ChoiceDisplayName("Extreme")]
    Extreme,
}

public enum HangmanGamemodeChoice
{
    [ChoiceDisplayName("Characters")]
    Characters,
    [ChoiceDisplayName("Animes")]
    Animes,
}

public enum GamemodeHoLChoice
{
    [ChoiceDisplayName("Score")]
    Score,
    [ChoiceDisplayName("Popularity")]
    Popularity,
}

public static class GameChoiceMapper
{
    public static Gamemode ToModel(this GamemodeChoice choice) => (Gamemode)(int)choice;

    public static Difficulty ToModel(this DifficultyChoice choice) => (Difficulty)(int)choice;

    public static HangmanGamemode ToModel(this HangmanGamemodeChoice choice) => (HangmanGamemode)(int)choice;

    public static GamemodeHoL ToModel(this GamemodeHoLChoice choice) => (GamemodeHoL)(int)choice;
}
