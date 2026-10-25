using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Commands.Framework.Choices;

// Mirror of the domain enums with the labels Discord shows. The Yumiko.Model enums cannot
// carry DSharpPlus attributes because that layer does not reference packages.
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
    [ChoiceDisplayName("Anime by genre")]
    Genres,
    [ChoiceDisplayName("Anime studio")]
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

public enum TriviaPlayersChoice
{
    [ChoiceDisplayName("Singleplayer")]
    Singleplayer,
    [ChoiceDisplayName("Multiplayer")]
    Multiplayer,
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

    public static TriviaPlayers ToModel(this TriviaPlayersChoice choice) => (TriviaPlayers)(int)choice;

    public static HangmanGamemode ToModel(this HangmanGamemodeChoice choice) => (HangmanGamemode)(int)choice;

    public static GamemodeHoL ToModel(this GamemodeHoLChoice choice) => (GamemodeHoL)(int)choice;
}
