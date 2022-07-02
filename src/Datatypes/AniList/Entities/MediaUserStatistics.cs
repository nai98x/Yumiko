using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaUserStatistics
    {
        [JsonProperty("status")]
        public MediaListStatus Status { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("startedAt")]
        public FuzzyDate StartedAt { get; set; } = null!;

        [JsonProperty("completedAt")]
        public FuzzyDate CompletedAt { get; set; } = null!;

        [JsonProperty("notes")]
        public string? Notes { get; set; }

        [JsonProperty("score")]
        public decimal Score { get; set; }

        [JsonProperty("repeat")]
        public int Repeat { get; set; }

        [JsonProperty("media")]
        public MediaEpisodes Media { get; set; } = null!;

        [JsonProperty("user")]
        public User User { get; set; } = null!;
    }
}
