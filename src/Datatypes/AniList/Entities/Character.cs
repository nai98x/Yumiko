using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Character
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public AniListName Name { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("animes")]
        public CharacterMediaNodes Animes { get; set; }

        [JsonProperty("mangas")]
        public CharacterMediaNodes Mangas { get; set; }
    }
}
