using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class NodesStaff
    {
        [JsonProperty("nodes")]
        public List<ProfileStaffNode>? Nodes { get; set; }
    }
}
