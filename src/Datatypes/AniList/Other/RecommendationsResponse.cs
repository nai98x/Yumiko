using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class RecommendationsResponse
    {
        [JsonProperty("User")]
        public Profile? User { get; set; }

        [JsonProperty("MediaListCollection")]
        public MediaListCollection? Recommendations { get; set; }
    }
}
