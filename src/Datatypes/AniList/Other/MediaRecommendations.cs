using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaRecommendations
    {
        [JsonProperty("recommendations")]
        public Recommendations Recommendations { get; set; }
    }
}
