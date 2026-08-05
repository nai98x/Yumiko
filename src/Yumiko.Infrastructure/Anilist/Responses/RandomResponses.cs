using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses;

// DTOs of the two "random" queries. They are minimal
// projections: only the fields the query asks for.
internal sealed class SimpleCharacterPageResponse
{
    [JsonProperty("Page")]
    public SimpleCharacterPage? Page { get; set; }
}

internal sealed class SimpleCharacterPage
{
    [JsonProperty("characters")]
    public List<SimpleCharacter>? Characters { get; set; }
}

internal sealed class SimpleCharacter
{
    [JsonProperty("name")]
    public SimpleName? Name { get; set; }

    [JsonProperty("image")]
    public SimpleImage? Image { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }

    [JsonProperty("media")]
    public SimpleMediaNodes? Media { get; set; }
}

internal sealed class SimpleName
{
    [JsonProperty("full")]
    public string? Full { get; set; }
}

internal sealed class SimpleImage
{
    [JsonProperty("large")]
    public string? Large { get; set; }
}

internal sealed class SimpleMediaNodes
{
    [JsonProperty("nodes")]
    public List<SimpleMedia>? Nodes { get; set; }
}

internal sealed class SimpleMediaPageResponse
{
    [JsonProperty("Page")]
    public SimpleMediaPage? Page { get; set; }
}

internal sealed class SimpleMediaPage
{
    [JsonProperty("media")]
    public List<SimpleMedia>? Media { get; set; }
}

internal sealed class SimpleMedia
{
    [JsonProperty("title")]
    public SimpleTitle? Title { get; set; }

    [JsonProperty("coverImage")]
    public SimpleCoverImage? CoverImage { get; set; }

    [JsonProperty("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonProperty("favourites")]
    public int Favourites { get; set; }
}

internal sealed class SimpleTitle
{
    [JsonProperty("romaji")]
    public string? Romaji { get; set; }

    [JsonProperty("english")]
    public string? English { get; set; }
}

internal sealed class SimpleCoverImage
{
    [JsonProperty("large")]
    public string? Large { get; set; }
}
