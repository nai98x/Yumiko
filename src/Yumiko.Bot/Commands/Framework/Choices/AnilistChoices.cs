using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Commands.Framework.Choices;

// Enums de presentación: solo existen para que Discord muestre etiquetas lindas en el selector.
// Los valores reales de la API de AniList viven en Yumiko.Model.Enum, que no conoce DSharpPlus.
public enum MediaUserStatusChoice
{
    [ChoiceDisplayName("Current")]
    CURRENT,
    [ChoiceDisplayName("Planning")]
    PLANNING,
    [ChoiceDisplayName("Completed")]
    COMPLETED,
    [ChoiceDisplayName("Dropped")]
    DROPPED,
    [ChoiceDisplayName("Paused")]
    PAUSED,
    [ChoiceDisplayName("Repeating")]
    REPEATING,
}

public enum MediaUserSortChoice
{
    [ChoiceDisplayName("Score")]
    SCORE_DESC,
    [ChoiceDisplayName("Popularity")]
    MEDIA_POPULARITY_DESC,
    [ChoiceDisplayName("Title")]
    MEDIA_TITLE_DESC,
}

public enum MediaTitleTypeChoice
{
    [ChoiceDisplayName("Romaji")]
    ROMAJI,
    [ChoiceDisplayName("English")]
    ENGLISH,
    [ChoiceDisplayName("Native")]
    NATIVE,
}

public enum MediaTypeChoice
{
    [ChoiceDisplayName("Anime")]
    ANIME,
    [ChoiceDisplayName("Manga")]
    MANGA,
}

public static class AnilistChoiceMapper
{
    public static MediaUserStatus ToModel(this MediaUserStatusChoice choice) => (MediaUserStatus)(int)choice;

    public static MediaUserSort ToModel(this MediaUserSortChoice choice) => (MediaUserSort)(int)choice;

    public static MediaTitleType ToModel(this MediaTitleTypeChoice choice) => (MediaTitleType)(int)choice;

    public static MediaType ToModel(this MediaTypeChoice choice) => (MediaType)(int)choice;
}
