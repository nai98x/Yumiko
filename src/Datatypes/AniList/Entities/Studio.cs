using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Studio
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("siteUrl")]
        public string? SiteUrl { get; set; }

        [JsonProperty("IsAnimationStudio")]
        public bool? isAnimationStudio { get; set; }
    }
}
