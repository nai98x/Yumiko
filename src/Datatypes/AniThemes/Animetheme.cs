using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Animetheme
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sequence")]
        public long? Sequence { get; set; }

        [JsonProperty("group")]
        public object Group { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public object DeletedAt { get; set; }

        [JsonProperty("animethemeentries")]
        public List<AnimeThemeEntry> Animethemeentries { get; set; }

        public string? GetSequence()
        {
            if (Sequence == null) return null;

            string s = Sequence.ToString()!;
            while (s.Length < 2) s = "0" + s;
            return s;
        }
    }
}