using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class NodesMedia
    {
        [JsonProperty("nodes")]
        public List<ProfileMediaNode>? Nodes { get; set; }
    }
}
