using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Search
    {
        [JsonProperty("anime")]
        public List<AnimeAniTheme> Anime { get; set; }

        [JsonProperty("animethemes")]
        public List<Animetheme> Animethemes { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("series")]
        public List<Series> Series { get; set; }

        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }

        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }
    }
}
