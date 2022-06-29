using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ExternalLink
    {
        [JsonProperty("site")]
        public string? Site { get; set; }

        [JsonProperty("url")]
        public Uri? Url { get; set; }
    }
}
