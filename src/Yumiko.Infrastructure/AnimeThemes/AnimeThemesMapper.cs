using Yumiko.Infrastructure.AnimeThemes.Responses;
using Yumiko.Model.Entities.AnimeThemes;

namespace Yumiko.Infrastructure.AnimeThemes;

internal static class AnimeThemesMapper
{
    internal static List<AnimeAniTheme> ToAnime(List<AnimeResponse>? anime) =>
        anime is null ? [] : [.. anime.Select(ToAnime)];

    private static AnimeAniTheme ToAnime(AnimeResponse anime) => new()
    {
        Id = anime.Id,
        Name = anime.Title?.Romaji ?? anime.Title?.English ?? anime.Slug ?? string.Empty,
        Slug = anime.Slug ?? string.Empty,
        Year = anime.Year,
        Season = anime.SeasonLocalized ?? string.Empty,
        Synopsis = anime.Synopsis ?? string.Empty,
        Animethemes = anime.Animethemes is null ? [] : [.. anime.Animethemes.Select(ToTheme)],
    };

    private static Animetheme ToTheme(AnimeThemeResponse theme) => new()
    {
        Id = theme.Id,
        Type = theme.Type ?? string.Empty,
        Sequence = theme.Sequence,
        Slug = theme.Slug ?? string.Empty,
        Animethemeentries = theme.Animethemeentries is null ? [] : [.. theme.Animethemeentries.Select(ToEntry)],
    };

    private static AnimeThemeEntry ToEntry(AnimeThemeEntryResponse entry) => new()
    {
        Id = entry.Id,
        Version = entry.Version,
        Episodes = entry.Episodes ?? string.Empty,
        Notes = entry.Notes ?? string.Empty,
        Nsfw = entry.Nsfw,
        Spoiler = entry.Spoiler,
        Videos = entry.Videos?.Nodes is null ? [] : [.. entry.Videos.Nodes.Select(ToVideo)],
    };

    private static Video ToVideo(VideoResponse video) => new()
    {
        Id = video.Id,
        Basename = video.Basename ?? string.Empty,
        Link = video.Link ?? string.Empty,
    };
}
