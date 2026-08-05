using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses;

// Projection of the character pool of the games.
internal sealed class CharacterPoolResponse
{
    [JsonProperty("Page")]
    public CharacterPoolPage? Page { get; set; }
}

internal sealed class CharacterPoolPage
{
    [JsonProperty("characters")]
    public List<CharacterPoolCharacter>? Characters { get; set; }

    [JsonProperty("pageInfo")]
    public CharacterPoolPageInfo? PageInfo { get; set; }
}

internal sealed class CharacterPoolPageInfo
{
    [JsonProperty("hasNextPage")]
    public bool HasNextPage { get; set; }
}

internal sealed class CharacterPoolCharacter
{
    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }

    [JsonProperty("name")]
    public CharacterPoolName? Name { get; set; }

    [JsonProperty("image")]
    public CharacterPoolImage? Image { get; set; }

    [JsonProperty("media")]
    public CharacterPoolMediaConnection? Media { get; set; }
}

internal sealed class CharacterPoolName
{
    [JsonProperty("first")]
    public string? First { get; set; }

    [JsonProperty("last")]
    public string? Last { get; set; }

    [JsonProperty("full")]
    public string? Full { get; set; }
}

internal sealed class CharacterPoolImage
{
    [JsonProperty("large")]
    public string? Large { get; set; }
}

internal sealed class CharacterPoolMediaConnection
{
    [JsonProperty("nodes")]
    public List<CharacterPoolMediaNode>? Nodes { get; set; }
}

internal sealed class CharacterPoolMediaNode
{
    [JsonProperty("title")]
    public CharacterPoolMediaTitle? Title { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }
}

internal sealed class CharacterPoolMediaTitle
{
    [JsonProperty("romaji")]
    public string? Romaji { get; set; }
}
