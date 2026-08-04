
namespace Yumiko.Model.Entities.Anilist
{
    public class RecommendationNode
    {
        public int Rating { get; set; }

        public Media MediaRecommendation { get; set; } = null!;
    }
}
