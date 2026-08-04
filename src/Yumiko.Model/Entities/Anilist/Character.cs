
namespace Yumiko.Model.Entities.Anilist
{
    public class Character
    {
        public long Id { get; set; }

        public AniListName Name { get; set; } = null!;

        public CharacterImage Image { get; set; } = null!;

        public int Favourites { get; set; }

        public Uri SiteUrl { get; set; } = null!;

        public string? Description { get; set; }

        public CharacterMediaConnection Animes { get; set; } = null!;

        public CharacterMediaConnection Mangas { get; set; } = null!;
    }
}
