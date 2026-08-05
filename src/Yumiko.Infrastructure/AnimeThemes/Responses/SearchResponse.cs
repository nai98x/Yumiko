using Newtonsoft.Json;

namespace Yumiko.Infrastructure.AnimeThemes.Responses;

// Projection of the GraphQL search query. Many-to-many relationships (videos) come wrapped in a
// connection, so their items hang from `nodes`.
internal sealed class GraphQLEnvelope<T>
{
    [JsonProperty("data")]
    public T? Data { get; set; }

    [JsonProperty("errors")]
    public List<GraphQLErrorResponse>? Errors { get; set; }
}

internal sealed class GraphQLErrorResponse
{
    [JsonProperty("message")]
    public string? Message { get; set; }
}

internal sealed class SearchResponse
{
    [JsonProperty("search")]
    public SearchResultsResponse? Search { get; set; }
}

internal sealed class SearchResultsResponse
{
    [JsonProperty("anime")]
    public List<AnimeResponse>? Anime { get; set; }
}

internal sealed class AnimeResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("title")]
    public AnimeTitleResponse? Title { get; set; }

    [JsonProperty("slug")]
    public string? Slug { get; set; }

    [JsonProperty("year")]
    public long Year { get; set; }

    [JsonProperty("seasonLocalized")]
    public string? SeasonLocalized { get; set; }

    [JsonProperty("synopsis")]
    public string? Synopsis { get; set; }

    [JsonProperty("animethemes")]
    public List<AnimeThemeResponse>? Animethemes { get; set; }
}

internal sealed class AnimeTitleResponse
{
    [JsonProperty("romaji")]
    public string? Romaji { get; set; }

    [JsonProperty("english")]
    public string? English { get; set; }
}

internal sealed class AnimeThemeResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("sequence")]
    public long? Sequence { get; set; }

    [JsonProperty("slug")]
    public string? Slug { get; set; }

    [JsonProperty("animethemeentries")]
    public List<AnimeThemeEntryResponse>? Animethemeentries { get; set; }
}

internal sealed class AnimeThemeEntryResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("version")]
    public long? Version { get; set; }

    [JsonProperty("episodes")]
    public string? Episodes { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("nsfw")]
    public bool Nsfw { get; set; }

    [JsonProperty("spoiler")]
    public bool Spoiler { get; set; }

    [JsonProperty("videos")]
    public VideoConnectionResponse? Videos { get; set; }
}

internal sealed class VideoConnectionResponse
{
    [JsonProperty("nodes")]
    public List<VideoResponse>? Nodes { get; set; }
}

internal sealed class VideoResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("basename")]
    public string? Basename { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }
}
