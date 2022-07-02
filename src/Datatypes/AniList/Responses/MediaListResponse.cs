using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListResponse
    {
        [JsonProperty("MediaList")]
        public MediaUserStatistics? MediaList { get; set; }
    }
}
