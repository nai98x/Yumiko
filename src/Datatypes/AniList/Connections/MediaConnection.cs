using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaConnection
    {
        [JsonProperty("nodes")]
        public List<MediaNode>? Nodes { get; set; }
    }
}
