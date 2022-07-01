using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaEntry
    {
        [JsonProperty("mediaId")]
        public int MediaId { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("status")]
        public MediaListStatus Status { get; set; }

        [JsonProperty("media")]
        public MediaRecommendations Media { get; set; } = null!;
    }
}
