using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaTitle
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; } = null!;

        [JsonProperty("english")]
        public string? English { get; set; }

        [JsonProperty("native")]
        public string? Native { get; set; }
    }
}
