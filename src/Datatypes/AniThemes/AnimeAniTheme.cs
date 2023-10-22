using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class AnimeAniTheme
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public object DeletedAt { get; set; }

        [JsonProperty("animethemes")]
        public List<Animetheme> Animethemes { get; set; }
    }
}
