using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterMediaConnection
    {
        [JsonProperty("nodes")]
        public List<MediaNode>? Nodes { get; set; }
    }
}
