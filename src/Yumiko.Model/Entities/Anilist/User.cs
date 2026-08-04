
namespace Yumiko.Model.Entities.Anilist
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public Uri SiteUrl { get; set; } = null!;

        public ProfileAvatar Avatar { get; set; } = null!;

        public Uri? BannerImage { get; set; }

        public ProfileOptions Options { get; set; } = null!;

        public ProfileStatistics Statistics { get; set; } = null!;

        public ProfileFavourites Favourites { get; set; } = null!;

        public MediaListOptions MediaListOptions { get; set; } = null!;
    }
}
