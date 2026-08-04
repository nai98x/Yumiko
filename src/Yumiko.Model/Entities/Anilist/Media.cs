
namespace Yumiko.Model.Entities.Anilist
{
    using Yumiko.Model.Enum;

    public class Media
    {
        public int Id { get; set; }

        public MediaTitle Title { get; set; } = null!;

        public List<string>? Synonyms { get; set; }

        public string? Description { get; set; }

        public Uri SiteUrl { get; set; } = null!;

        public MediaCoverImage CoverImage { get; set; } = null!;

        public string? BannerImage { get; set; }

        public MediaFormat? Format { get; set; }

        public int? Volumes { get; set; }

        public int? Chapters { get; set; }

        public int? Episodes { get; set; }

        public MediaStatus? Status { get; set; }

        public int? MeanScore { get; set; }

        public List<string>? Genres { get; set; }

        public int? SeasonYear { get; set; }

        public FuzzyDate StartDate { get; set; } = null!;

        public FuzzyDate EndDate { get; set; } = null!;

        public List<Tag>? Tags { get; set; }

        public StudioConnection? Studios { get; set; }

        public List<ExternalLink>? ExternalLinks { get; set; }

        public bool IsAdult { get; set; }
    }
}
