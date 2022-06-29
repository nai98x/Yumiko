using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListResponse
    {
        [JsonProperty("MediaList")]
        public MediaUser? MediaList { get; set; }
    }
}
