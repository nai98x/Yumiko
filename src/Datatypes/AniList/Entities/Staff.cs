using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Staff
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public AniListName Name { get; set; } = null!;

        [JsonProperty("image")]
        public CharacterImage Image { get; set; } = null!;

        [JsonProperty("languageV2")]
        public string? Language { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("age")]
        public int? Age { get; set; }

        [JsonProperty("dateOfBirth")]
        public FuzzyDate DateOfBirth { get; set; } = null!;

        [JsonProperty("dateOfDeath")]
        public FuzzyDate DateOfDeath { get; set; } = null!;
    }
}
