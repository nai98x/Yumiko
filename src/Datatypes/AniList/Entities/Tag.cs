using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Tag
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("isMediaSpoiler")]
        public bool? IsMediaSpoiler { get; set; }
    }
}
