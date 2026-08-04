
namespace Yumiko.Model.Entities.Anilist
{
    public class Studio
    {
        public string Name { get; set; } = null!;

        public Uri SiteUrl { get; set; } = null!;

        public bool? IsAnimationStudio { get; set; }
    }
}
