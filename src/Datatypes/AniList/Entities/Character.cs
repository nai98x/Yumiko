using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Character
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public AniListName Name { get; set; } = null!;

        [JsonProperty("image")]
        public CharacterImage Image { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("animes")]
        public CharacterMediaNodes Animes { get; set; } = null!;

        [JsonProperty("mangas")]
        public CharacterMediaNodes Mangas { get; set; } = null!;
    }
}
