
namespace Yumiko.Model.Entities.Anilist
{
    using Yumiko.Model.Enum;

    public class MediaNode
    {
        public int Id { get; set; }

        public MediaType Type { get; set; }

        public MediaTitle Title { get; set; } = null!;

        public Uri SiteUrl { get; set; } = null!;
    }
}
