using DSharpPlus;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Games;

/// <summary>
/// An entry of the trivia pool already resolved to text: the name that goes on the button and the description
/// that reveals the answer. It lets the runner treat characters and media alike.
/// </summary>
public sealed record TriviaItem(string Name, string? Image, string Description);

public static class TriviaItems
{
    public static List<TriviaItem> FromCharacters(IEnumerable<CharacterOld> characters, Loc loc) =>
    [
        .. characters
            .Where(p => !string.IsNullOrEmpty(p.NameFull))
            .Select(p => new TriviaItem(
                p.NameFull!,
                p.Image,
                loc.Format(
                    Keys.the_character_is,
                    Formatter.Bold($"[{p.NameFull}]({p.SiteUrl})"),
                    $"[{p.MainAnime?.TitleRomaji}]({p.MainAnime?.SiteUrl})"))),
    ];

    public static List<TriviaItem> FromMedia(IEnumerable<Anime> media, Gamemode gamemode, Loc loc) =>
    [
        .. media
            .Select(m => Build(m, gamemode, loc))
            .Where(item => item is not null)
            .Select(item => item!),
    ];

    private static TriviaItem? Build(Anime media, Gamemode gamemode, Loc loc) => gamemode switch
    {
        Gamemode.Studios => media.Studios is { Count: > 0 } studios
            ? new TriviaItem(studios[0].Name, media.Image, StudiosDescription(media, studios, loc))
            : null,
        Gamemode.Protagonists => media.Characters is { Count: > 0 } characters && !string.IsNullOrEmpty(characters[0].NameFull)
            ? new TriviaItem(characters[0].NameFull!, media.Image, ProtagonistsDescription(media, characters, loc))
            : null,
        Gamemode.Mangas => new TriviaItem(media.TitleRomaji, media.Image, WithEnglishTitle(loc.Format(Keys.the_manga_is, Formatter.Bold(Link(media))), media, loc)),
        Gamemode.Genres => new TriviaItem(media.TitleRomaji, media.Image, GenreDescription(media, loc)),
        _ => new TriviaItem(media.TitleRomaji, media.Image, WithEnglishTitle(loc.Format(Keys.the_anime_is, Formatter.Bold(Link(media))), media, loc)),
    };

    private static string Link(Anime media) => $"[{media.TitleRomaji}]({media.SiteUrl})";

    private static string WithEnglishTitle(string description, Anime media, Loc loc) =>
        string.IsNullOrEmpty(media.TitleEnglish)
            ? description
            : $"{description}\n{loc[Keys.in_english]}: `{media.TitleEnglish}`";

    private static string StudiosDescription(Anime media, List<StudioOld> studios, Loc loc) =>
        $"{loc.Format(Keys.the_studios_of_are, Link(media))}\n" +
        string.Join("\n", studios.Select(e => $"- {Formatter.Bold($"[{e.Name}]({e.SiteUrl})")}"));

    private static string ProtagonistsDescription(Anime media, List<CharacterOld> characters, Loc loc) =>
        $"{loc.Format(Keys.the_protagonists_of_are, Link(media))}\n" +
        string.Join("\n", characters.Select(p => $"- {Formatter.Bold($"[{p.NameFull}]({p.SiteUrl})")}"));

    private static string GenreDescription(Anime media, Loc loc)
    {
        string description = WithEnglishTitle(loc.Format(Keys.the_anime_is, Link(media)), media, loc);

        if (media.RelatedMedia is not { Count: > 0 } related)
        {
            return description;
        }

        return $"{description}\n\n{loc[Keys.related]}:\n" +
            string.Join("\n", related.Select(r =>
                $"- {Formatter.Bold($"[{r.TitleRomaji}]({r.SiteUrl})")} ({r.Type.UppercaseFirst()})"));
    }
}
