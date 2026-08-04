
namespace Yumiko.Model.Entities.Anilist
{
    public class ProfileStatistics
    {
        public ProfileStatisticsAnime Anime { get; set; } = null!;

        public ProfileStatisticsManga Manga { get; set; } = null!;
    }
}
