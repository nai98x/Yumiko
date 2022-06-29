using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileStatistics
    {
        [JsonProperty("anime")]
        public ProfileStatisticsAnime Anime { get; set; } = null!;

        [JsonProperty("manga")]
        public ProfileStatisticsManga Manga { get; set; } = null!;
    }
}
