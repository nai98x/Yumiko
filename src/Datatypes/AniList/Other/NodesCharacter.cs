using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class NodesCharacter
    {
        [JsonProperty("nodes")]
        public List<ProfileCharacterNode>? Nodes { get; set; }
    }
}
