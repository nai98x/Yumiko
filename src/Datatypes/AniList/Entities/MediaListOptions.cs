using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListOptions
    {
        [JsonProperty("scoreFormat")]
        public string ScoreFormat { get; set; } = null!;
    }
}
