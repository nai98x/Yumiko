using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class AnimeThemeEntry
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("version")]
        public long? Version { get; set; }

        [JsonProperty("episodes")]
        public string Episodes { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("spoiler")]
        public bool Spoiler { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public object DeletedAt { get; set; }

        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }
    }
}