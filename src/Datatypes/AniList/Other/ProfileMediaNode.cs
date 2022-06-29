using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileMediaNode
    {
        [JsonProperty("title")]
        public MediaTitle Title { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;
    }
}
