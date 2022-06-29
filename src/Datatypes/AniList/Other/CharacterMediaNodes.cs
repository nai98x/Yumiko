using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterMediaNodes
    {
        [JsonProperty("nodes")]
        public List<CharacterMediaNode> Nodes { get; set; }
    }
}
