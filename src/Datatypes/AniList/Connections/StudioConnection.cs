using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class StudioConnection
    {
        [JsonProperty("nodes")]
        public List<Studio>? Nodes { get; set; }
    }
}
