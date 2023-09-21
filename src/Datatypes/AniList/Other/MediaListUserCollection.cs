using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaListUserCollection
    {
        [JsonProperty("lists")]
        public List<MediaUserList>? Lists { get; set; }
    }
}
