
namespace Yumiko.Model.Entities.Anilist
{
    using Yumiko.Model.Enum;

    public class MediaEntry
    {
        public int MediaId { get; set; }

        public int? Score { get; set; }

        public MediaListStatus Status { get; set; }

        public MediaRecommendations Media { get; set; } = null!;
    }
}
