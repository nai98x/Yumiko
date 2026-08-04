
namespace Yumiko.Model.Entities.Anilist
{
    public class ProfileStatisticsAnime
    {
        public int Count { get; set; }

        public int EpisodesWatched { get; set; }

        public decimal MeanScore { get; set; }

        public decimal StandardDeviation { get; set; }
    }
}
