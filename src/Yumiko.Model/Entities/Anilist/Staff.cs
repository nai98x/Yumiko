
namespace Yumiko.Model.Entities.Anilist
{
    public class Staff
    {
        public int Id { get; set; }

        public AniListName Name { get; set; } = null!;

        public CharacterImage Image { get; set; } = null!;

        public string? LanguageV2 { get; set; }

        public string? Description { get; set; }

        public Uri SiteUrl { get; set; } = null!;

        public string? Gender { get; set; }

        public int? Age { get; set; }

        public FuzzyDate DateOfBirth { get; set; } = null!;

        public FuzzyDate DateOfDeath { get; set; } = null!;
    }
}
