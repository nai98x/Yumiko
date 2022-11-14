using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Video
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("basename")]
        public string Basename { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("resolution")]
        public long Resolution { get; set; }

        [JsonProperty("nc")]
        public bool Nc { get; set; }

        [JsonProperty("subbed")]
        public bool Subbed { get; set; }

        [JsonProperty("lyrics")]
        public bool Lyrics { get; set; }

        [JsonProperty("uncen")]
        public bool Uncen { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("overlap")]
        public string Overlap { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public string DeletedAt { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
