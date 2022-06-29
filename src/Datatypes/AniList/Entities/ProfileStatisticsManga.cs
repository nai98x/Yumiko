using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileStatisticsManga
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("chaptersRead")]
        public int ChaptersRead { get; set; }

        [JsonProperty("meanScore")]
        public decimal MeanScore { get; set; }
    }
}
