using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class RecommendationNode
    {
        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("mediaRecommendation")]
        public Media MediaRecommendation { get; set; } = null!;
    }
}
