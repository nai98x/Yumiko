using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterConnection
    {
        [JsonProperty("nodes")]
        public List<ProfileCharacterNode>? Nodes { get; set; }
    }
}
