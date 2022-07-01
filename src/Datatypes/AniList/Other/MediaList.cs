using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaList
    {
        [JsonProperty("entries")]
        public List<MediaEntry> Entries { get; set; }
    }
}
