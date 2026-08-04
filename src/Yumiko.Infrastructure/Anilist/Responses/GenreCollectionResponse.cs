using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses;

internal sealed class GenreCollectionResponse
{
    [JsonProperty("GenreCollection")]
    public List<string>? GenreCollection { get; set; }
}
