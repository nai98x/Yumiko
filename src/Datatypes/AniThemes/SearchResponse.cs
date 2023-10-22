using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class SearchResponse
    {
        [JsonProperty("anime")]
        public List<AnimeAniTheme> Anime { get; set; }
    }
}
