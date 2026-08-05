using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses;

// Projection of the query that feeds the media pool of the games (trivia, hangman, HoL).
internal sealed class GamePoolResponse
{
    [JsonProperty("Page")]
    public GamePoolPage? Page { get; set; }
}

internal sealed class GamePoolPage
{
    [JsonProperty("media")]
    public List<GamePoolMedia>? Media { get; set; }

    [JsonProperty("pageInfo")]
    public GamePoolPageInfo? PageInfo { get; set; }
}

internal sealed class GamePoolPageInfo
{
    [JsonProperty("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonProperty("lastPage")]
    public int LastPage { get; set; }
}

internal sealed class GamePoolMedia
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }

    [JsonProperty("title")]
    public GamePoolTitle? Title { get; set; }

    /// <summary>AniList returns <c>null</c> when the media does not have a score yet.</summary>
    [JsonProperty("averageScore")]
    public int? AverageScore { get; set; }

    [JsonProperty("synonyms")]
    public List<string>? Synonyms { get; set; }

    [JsonProperty("coverImage")]
    public GamePoolCoverImage? CoverImage { get; set; }

    [JsonProperty("characters")]
    public GamePoolCharacterConnection? Characters { get; set; }

    [JsonProperty("studios")]
    public GamePoolStudioConnection? Studios { get; set; }

    [JsonProperty("relations")]
    public GamePoolRelationConnection? Relations { get; set; }
}

internal sealed class GamePoolTitle
{
    [JsonProperty("romaji")]
    public string? Romaji { get; set; }

    [JsonProperty("english")]
    public string? English { get; set; }
}

internal sealed class GamePoolCoverImage
{
    [JsonProperty("large")]
    public string? Large { get; set; }
}

internal sealed class GamePoolCharacterConnection
{
    [JsonProperty("nodes")]
    public List<GamePoolCharacter>? Nodes { get; set; }
}

internal sealed class GamePoolCharacter
{
    [JsonProperty("name")]
    public GamePoolCharacterName? Name { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }
}

internal sealed class GamePoolCharacterName
{
    [JsonProperty("first")]
    public string? First { get; set; }

    [JsonProperty("last")]
    public string? Last { get; set; }

    [JsonProperty("full")]
    public string? Full { get; set; }
}

internal sealed class GamePoolStudioConnection
{
    [JsonProperty("nodes")]
    public List<GamePoolStudio>? Nodes { get; set; }
}

internal sealed class GamePoolStudio
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }

    [JsonProperty("isAnimationStudio")]
    public bool IsAnimationStudio { get; set; }
}

internal sealed class GamePoolRelationConnection
{
    [JsonProperty("edges")]
    public List<GamePoolRelationEdge>? Edges { get; set; }
}

internal sealed class GamePoolRelationEdge
{
    [JsonProperty("relationType")]
    public string? RelationType { get; set; }

    [JsonProperty("node")]
    public GamePoolRelationNode? Node { get; set; }
}

internal sealed class GamePoolRelationNode
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("title")]
    public GamePoolTitle? Title { get; set; }

    [JsonProperty("synonyms")]
    public List<string>? Synonyms { get; set; }
}
