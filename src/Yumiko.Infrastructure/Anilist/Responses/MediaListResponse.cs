using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class MediaListResponse
    {
        [JsonProperty("MediaList")]
        public MediaUserStatistics? MediaList { get; set; }
    }
}
