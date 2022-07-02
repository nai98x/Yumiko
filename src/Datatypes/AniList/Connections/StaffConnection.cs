using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class StaffConnection
    {
        [JsonProperty("nodes")]
        public List<ProfileStaffNode>? Nodes { get; set; }
    }
}
