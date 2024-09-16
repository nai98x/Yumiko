using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class RelationsConnection
    {
        [JsonProperty("nodes")]
        public List<RelationNode>? Nodes { get; set; }
    }
}
