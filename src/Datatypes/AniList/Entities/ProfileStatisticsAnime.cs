using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileStatisticsAnime
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("episodesWatched")]
        public int EpisodesWatched { get; set; }

        [JsonProperty("meanScore")]
        public decimal MeanScore { get; set; }
    }
}
