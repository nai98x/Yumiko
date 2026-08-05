using DSharpPlus.Commands.Processors.SlashCommands;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.AnimeThemes;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Helpers;

/// <summary>Chains the three choices of <c>/anitheme</c>: anime → theme (OP/ED) → version.</summary>
public sealed class AnithemeSelector(IAnimeThemesClient client, DiscordInteractivity discordInteractivity)
{
    public async Task<AnithemeData?> SearchAsync(SlashCommandContext ctx, string search, Loc loc)
    {
        List<AnimeAniTheme> animeResults = await client.SearchAsync(search);

        if (await ChooseAsync(ctx, animeResults, a => new TitleDescription { Title = a.Name, Description = $"{a.Season} {a.Year}" }, loc, SortAnime) is not { } anime)
        {
            return null;
        }

        List<Animetheme> themes = [.. SortThemes(anime.Animethemes)];

        if (await ChooseAsync(ctx, themes, ThemeTitle, loc) is not { } theme)
        {
            return null;
        }

        if (await ChooseAsync(ctx, theme.Animethemeentries, VersionTitle, loc, l => [.. l.OrderBy(e => $"v{e.Version}", StringComparer.Ordinal)]) is not { } version)
        {
            return null;
        }

        Video? video = version.Videos.FirstOrDefault();

        return video is null ? null : new AnithemeData(anime, theme, version, video);
    }

    private async Task<T?> ChooseAsync<T>(
        SlashCommandContext ctx,
        List<T> list,
        Func<T, TitleDescription> toOption,
        Loc loc,
        Func<List<T>, List<T>>? sort = null)
        where T : class
    {
        if (list.Count == 0)
        {
            return null;
        }

        List<T> sorted = sort is null ? list : sort(list);
        int? chosen = await discordInteractivity.ChooseAsync(ctx, [.. sorted.Select(toOption)], loc);

        return chosen is null ? null : sorted[chosen.Value];
    }

    private static List<AnimeAniTheme> SortAnime(List<AnimeAniTheme> animeResults) =>
        [.. animeResults.OrderBy(a => $"{a.Name} ({a.Season} {a.Year})", StringComparer.Ordinal)];

    /// <summary>
    /// Sorts by descending type (OP before ED) and by sequence inside each type, and discards
    /// the themes whose slug carries a suffix (alternative versions without their own entry).
    /// </summary>
    private static IEnumerable<Animetheme> SortThemes(List<Animetheme> themes) =>
        themes
            .Where(t => string.IsNullOrEmpty(t.Slug) || int.TryParse(t.Slug[2..], out _))
            .OrderByDescending(t => t.Type, StringComparer.Ordinal)
            .ThenBy(t => t.GetSequence() ?? "00", StringComparer.Ordinal);

    private static TitleDescription ThemeTitle(Animetheme theme) => new()
    {
        Title = theme.Sequence is null ? theme.Type : $"{theme.Type} {theme.GetSequence()}",
    };

    private static TitleDescription VersionTitle(AnimeThemeEntry entry)
    {
        string title = $"v{entry.Version}";

        if (entry.Spoiler)
        {
            title += " (SPOILER)";
        }

        if (entry.Nsfw)
        {
            title += " (NSFW)";
        }

        return new TitleDescription { Title = title };
    }
}
