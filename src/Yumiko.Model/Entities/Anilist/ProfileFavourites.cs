
namespace Yumiko.Model.Entities.Anilist
{
    public class ProfileFavourites
    {
        public MediaConnection Anime { get; set; } = null!;

        public MediaConnection Manga { get; set; } = null!;

        public CharacterConnection Characters { get; set; } = null!;

        public StaffConnection Staff { get; set; } = null!;

        public StudioConnection Studios { get; set; } = null!;
    }
}
