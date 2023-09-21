using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaUserList
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("entries")]
        public List<MediaUserEntry> Entries { get; set; }
    }
}
