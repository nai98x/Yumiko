using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaCoverImage
    {
        [JsonProperty("large")]
        public string? Large { get; set; }

        [JsonProperty("medium")]
        public string? Medium { get; set; }
    }
}
