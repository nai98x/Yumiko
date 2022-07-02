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

        [JsonProperty("favourites")]
        public int Favorites { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("animes")]
        public CharacterMediaConnection Animes { get; set; } = null!;

        [JsonProperty("mangas")]
        public CharacterMediaConnection Mangas { get; set; } = null!;
    }
}
