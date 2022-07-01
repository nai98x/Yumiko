using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListCollection
    {
        [JsonProperty("lists")]
        public List<MediaList> Lists { get; set; }
    }
}
