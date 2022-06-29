using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileCharacterNode
    {
        [JsonProperty("name")]
        public AniListName Name { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;
    }
}
