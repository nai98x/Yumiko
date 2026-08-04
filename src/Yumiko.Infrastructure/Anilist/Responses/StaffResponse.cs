using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class StaffResponse
    {
        [JsonProperty("staff")]
        public List<Staff>? Staffs { get; set; }
    }
}
