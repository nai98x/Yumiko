using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class ViewerResponse
    {
        [JsonProperty("Viewer")]
        public User? Viewer { get; set; }
    }
}
