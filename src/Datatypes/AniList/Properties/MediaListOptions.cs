using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListOptions
    {
        [JsonProperty("scoreFormat")]
        public ScoreFormat ScoreFormat { get; set; }
    }
}
