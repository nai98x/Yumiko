using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class StaffPageResponse
    {
        [JsonProperty("Page")]
        public StaffResponse? Page { get; set; }
    }
}
