using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class RelationNode
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public MediaType type { get; set; }
    }
}
