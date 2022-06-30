using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class StaffResponse
    {
        [JsonProperty("staff")]
        public List<Staff>? Staffs { get; set; }
    }
}
