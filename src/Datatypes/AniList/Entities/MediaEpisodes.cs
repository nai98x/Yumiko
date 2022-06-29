using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaEpisodes
    {
        [JsonProperty("episodes")]
        public int? Episodes { get; set; }

        [JsonProperty("chapters")]
        public int? Chapters { get; set; }
    }
}
