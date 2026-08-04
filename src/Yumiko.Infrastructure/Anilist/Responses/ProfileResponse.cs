using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class ProfileResponse
    {
        [JsonProperty("User")]
        public User? User { get; set; }
    }
}
