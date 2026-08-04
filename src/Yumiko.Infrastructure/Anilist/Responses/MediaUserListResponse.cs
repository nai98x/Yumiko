using Newtonsoft.Json;

namespace Yumiko.Infrastructure.Anilist.Responses
{
    using Yumiko.Model.Entities.Anilist;

    public class MediaUserListResponse
    {
        [JsonProperty("MediaListCollection")]
        public MediaListUserCollection MediaListCollection { get; set; }
    }
}
