using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaUserListResponse
    {
        [JsonProperty("MediaListCollection")]
        public MediaListUserCollection MediaListCollection { get; set; }
    }
}
