using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class RecommendationsConnection
    {
        [JsonProperty("nodes")]
        public List<RecommendationNode>? Nodes { get; set; }
    }
}
