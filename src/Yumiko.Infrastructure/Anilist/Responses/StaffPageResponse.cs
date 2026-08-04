using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class StaffPageResponse
    {
        [JsonProperty("Page")]
        public StaffResponse? Page { get; set; }
    }
}
