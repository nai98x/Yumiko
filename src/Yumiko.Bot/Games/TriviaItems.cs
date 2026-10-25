using DSharpPlus;
using Yumiko.Application.Anilist;
using Yumiko.Application.Games;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Games;

/// <summary>
/// An entry of the trivia pool already resolved to text: the name that goes on the button and the description
/// that reveals the answer. It lets the runner treat characters and media alike. The decoys of a round share
/// its <see cref="DecoyGroup"/>; <c>null</c> means any entry works as a decoy. The <see cref="Hint"/>, if any,
/// shows up halfway through the round.
/// </summary>
public sealed record TriviaItem(string Name, string? Image, string Description, string? DecoyGroup = null, string? Hint = null);

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
                    $"[{p.MainAnime?.TitleRomaji}]({p.MainAnime?.SiteUrl})"),
                TriviaRound.CharacterGenderGroup(p.Gender),
                string.IsNullOrEmpty(p.MainAnime?.TitleRomaji) || MediaTitles.ContainsName(p.MainAnime.TitleRomaji, p.NameFull)
                    ? null
                    : loc.Format(Keys.character_appears_in, Formatter.Bold(p.MainAnime.TitleRomaji))))
            .DistinctBy(item => item.Name, StringComparer.Ordinal),
    ];

    public static List<TriviaItem> FromMedia(IEnumerable<Anime> media, Gamemode gamemode, Loc loc) =>
    [
        .. media
            .Select(m => Build(m, gamemode, loc))
            .Where(item => item is not null)
            .Select(item => item!)
            // Different entries can resolve to the same name (several anime by the same studio, for
            // instance), and two options with the same name make the round unanswerable.
            .DistinctBy(item => item.Name, StringComparer.Ordinal),
    ];

    private static TriviaItem? Build(Anime media, Gamemode gamemode, Loc loc) => gamemode switch
    {
        Gamemode.Studios => media.Studios is { Count: > 0 } studios
            ? new TriviaItem(studios[0].Name, media.Image, StudiosDescription(media, studios, loc), Hint: StudioHint(media, studios[0], loc))
            : null,
        Gamemode.Protagonists => media.Characters is { Count: > 0 } characters && !string.IsNullOrEmpty(characters[0].NameFull)
            ? new TriviaItem(characters[0].NameFull!, media.Image, ProtagonistsDescription(media, characters, loc), Hint: AnimeHint(media, characters[0], loc))
            : null,
        Gamemode.Mangas => new TriviaItem(media.TitleRomaji, media.Image, WithEnglishTitle(loc.Format(Keys.the_manga_is, Formatter.Bold(Link(media))), media, loc), Hint: ProtagonistHint(media, loc)),
        Gamemode.Genres => new TriviaItem(media.TitleRomaji, media.Image, GenreDescription(media, loc), Hint: ProtagonistHint(media, loc)),
        _ => new TriviaItem(media.TitleRomaji, media.Image, WithEnglishTitle(loc.Format(Keys.the_anime_is, Formatter.Bold(Link(media))), media, loc), Hint: ProtagonistHint(media, loc)),
    };

    // The most popular anime of a studio is often the one on the cover (or one of its seasons), which would
    // give nothing away.
    private static string? StudioHint(Anime media, StudioOld studio, Loc loc) =>
        studio.PopularMedia.FirstOrDefault(m => m.Id != media.Id && !MediaTitles.AreSameSeries(m.TitleRomaji, media.TitleRomaji)) is { } other
            ? loc.Format(Keys.studio_also_made, Formatter.Bold(other.TitleRomaji))
            : null;

    private static string? AnimeHint(Anime media, CharacterOld protagonist, Loc loc) =>
        MediaTitles.ContainsName(media.TitleRomaji, protagonist.NameFull)
            ? null
            : $"{loc[Keys.anime]}: {Formatter.Bold(media.TitleRomaji)}";

    private static string? ProtagonistHint(Anime media, Loc loc) =>
        media.Characters?.FirstOrDefault(c => !string.IsNullOrEmpty(c.NameFull) && !MediaTitles.ContainsName(media.TitleRomaji, c.NameFull)) is { } protagonist
            ? $"{loc[Keys.protagonist]}: {Formatter.Bold(protagonist.NameFull!)}"
            : null;

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

    private static string GenreDescription(Anime media, Loc loc) =>
        WithEnglishTitle(loc.Format(Keys.the_anime_is, Link(media)), media, loc);
}
