using Yumiko.Application.Helpers;
using Yumiko.Infrastructure.Anilist.Responses;
using Yumiko.Model.Entities;

namespace Yumiko.Infrastructure.Anilist;

internal static class GameMediaMapper
{
    internal static string Filtros(GameMediaQuery query)
    {
        List<string> filtros = [];

        if (query.Genre is { Length: > 0 } genre)
        {
            filtros.Add("type: ANIME");
            filtros.Add("sort: POPULARITY_DESC");
            filtros.Add($"genre: \"{genre}\"");
        }
        else
        {
            filtros.Add($"type: {System.Enum.GetName(query.Type)}");
            filtros.Add("sort: FAVOURITES_DESC");
        }

        if (!query.IncludeAdult)
        {
            filtros.Add("isAdult: false");
        }

        filtros.Add("format_not_in: [MUSIC]");

        if (query.ExcludeUnreleased)
        {
            filtros.Add("status_not_in: [NOT_YET_RELEASED]");
        }

        return string.Join(", ", filtros);
    }

    internal static GameMediaPage Map(GamePoolResponse? response, GameMediaQuery query)
    {
        GamePoolPage? page = response?.Page;

        return new GameMediaPage
        {
            Media = [.. (page?.Media ?? []).Select(item => MapMedia(item, query))],
            HasNextPage = page?.PageInfo?.HasNextPage ?? false,
            LastPage = page?.PageInfo?.LastPage ?? 0,
        };
    }

    private static Anime MapMedia(GamePoolMedia item, GameMediaQuery query)
    {
        Anime anime = new()
        {
            Id = item.Id,
            Image = item.CoverImage?.Large,
            TitleRomaji = item.Title?.Romaji!,
            TitleEnglish = item.Title?.English,
            TitleRomajiFormatted = TextHelper.RemoveSpecialCharacters(item.Title?.Romaji),
            TitleEnglishFormatted = TextHelper.RemoveSpecialCharacters(item.Title?.English),
            SiteUrl = item.SiteUrl,
            Type = item.Type,
            Favourites = item.Favourites,
            // -1 marks "no score".
            AvarageScore = item.AverageScore ?? -1,
            Synonyms = [.. (item.Synonyms ?? []).Select(TextHelper.RemoveSpecialCharacters)],
            Studios = [],
            Characters = [],
        };

        if (query.IncludeCharacters)
        {
            foreach (GamePoolCharacter character in item.Characters?.Nodes ?? [])
            {
                anime.Characters!.Add(new CharacterOld
                {
                    NameFull = character.Name?.Full,
                    NameFirst = character.Name?.First,
                    NameLast = character.Name?.Last,
                    SiteUrl = character.SiteUrl,
                    Favourites = character.Favourites,
                });
            }
        }

        if (query.IncludeStudios)
        {
            foreach (GamePoolStudio studio in (item.Studios?.Nodes ?? []).Where(e => e.IsAnimationStudio))
            {
                if (studio.Name is not { } name)
                {
                    continue;
                }

                anime.Studios!.Add(new StudioOld
                {
                    Name = name,
                    SiteUrl = studio.SiteUrl,
                    Favourites = studio.Favourites,
                    PopularMedia = [.. (studio.Media?.Nodes ?? [])
                        .Where(m => !string.IsNullOrEmpty(m.Title?.Romaji))
                        .Select(m => new Anime { Id = m.Id, TitleRomaji = m.Title!.Romaji! })],
                });
            }
        }

        return anime;
    }
}
