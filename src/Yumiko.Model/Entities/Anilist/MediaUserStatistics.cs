
namespace Yumiko.Model.Entities.Anilist
{
    using Yumiko.Model.Enum;

    public class MediaUserStatistics
    {
        public MediaListStatus Status { get; set; }

        public int Progress { get; set; }

        public FuzzyDate StartedAt { get; set; } = null!;

        public FuzzyDate CompletedAt { get; set; } = null!;

        public string? Notes { get; set; }

        public decimal Score { get; set; }

        public int Repeat { get; set; }

        public MediaEpisodes Media { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
