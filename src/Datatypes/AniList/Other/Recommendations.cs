using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Recommendations
    {
        [JsonProperty("nodes")]
        public List<RecommendationNode> Nodes { get; set; }
    }
}
