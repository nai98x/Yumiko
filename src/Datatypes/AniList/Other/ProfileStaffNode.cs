using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileStaffNode
    {
        [JsonProperty("name")]
        public AniListName Name { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;
    }
}
