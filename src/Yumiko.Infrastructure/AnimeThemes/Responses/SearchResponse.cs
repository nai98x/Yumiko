using Newtonsoft.Json;

namespace Yumiko.Infrastructure.AnimeThemes.Responses
{
    using Yumiko.Model.Entities.AnimeThemes;

    public class SearchResponse
    {
        [JsonProperty("anime")]
        public List<AnimeAniTheme> Anime { get; set; }
    }
}
