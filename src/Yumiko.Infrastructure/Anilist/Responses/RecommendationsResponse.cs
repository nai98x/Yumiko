using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class RecommendationsResponse
    {
        [JsonProperty("User")]
        public User? User { get; set; }

        [JsonProperty("MediaListCollection")]
        public MediaListCollection? Recommendations { get; set; }
    }
}
