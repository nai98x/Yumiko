using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterMediaNode
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public MediaTitle Title { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; }
    }
}
