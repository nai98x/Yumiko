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

        List<Anime> media = [];

        foreach (GamePoolMedia item in page?.Media ?? [])
        {
            Anime anime = MapMedia(item, query);

            // Media that does not provide what the game mode needs is discarded.
            bool matches = (!query.IncludeStudios && !query.IncludeCharacters)
                || (query.IncludeStudios && anime.Studios!.Count > 0)
                || (query.IncludeCharacters && anime.Characters!.Count > 0);

            if (matches)
            {
                media.Add(anime);
            }
        }

        return new GameMediaPage
        {
            Media = media,
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
            RelatedMedia = [],
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
                anime.Studios!.Add(new StudioOld
                {
                    Name = studio.Name,
                    SiteUrl = studio.SiteUrl,
                    Favourites = studio.Favourites,
                });
            }
        }

        if (query.IncludeRelatedMedia)
        {
            foreach (GamePoolRelationEdge edge in item.Relations?.Edges ?? [])
            {
                if (edge.Node is not { } node)
                {
                    continue;
                }

                anime.RelatedMedia!.Add(new Anime
                {
                    Relation = edge.RelationType,
                    Id = node.Id,
                    SiteUrl = node.SiteUrl,
                    Type = node.Type,
                    TitleRomaji = node.Title?.Romaji!,
                    TitleEnglish = node.Title?.English,
                    TitleRomajiFormatted = TextHelper.RemoveSpecialCharacters(node.Title?.Romaji),
                    TitleEnglishFormatted = TextHelper.RemoveSpecialCharacters(node.Title?.English),
                    Synonyms = [.. (node.Synonyms ?? []).Select(TextHelper.RemoveSpecialCharacters)],
                });
            }
        }

        return anime;
    }
}
