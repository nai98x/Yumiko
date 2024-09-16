using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaRecommendations
    {
        [JsonProperty("recommendations")]
        public RecommendationsConnection? Recommendations { get; set; }

        [JsonProperty("relations")]
        public RelationsConnection? Relations { get; set; }
    }
}
