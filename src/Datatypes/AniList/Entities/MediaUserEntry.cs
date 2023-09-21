using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaUserEntry
    {
        [JsonProperty("media")]
        public Media Media { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }
    }
}
