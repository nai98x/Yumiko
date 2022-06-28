namespace Yumiko.Datatypes
{
    public class MediaResponse
    {
        public AniListMedia Media { get; set; }

        public class AniListMedia : IMedia
        {
            public int? id { get; set; }

            public MediaTitle title { get; set; }

            public List<string> synonyms { get; set; }

            public string description { get; set; }

            public string siteUrl { get; set; }

            public MediaCoverImage coverImage { get; set; }

            public MediaFormat? format { get; set; }

            public int? volumes { get; set; }

            public int? chapters { get; set; }

            public int? episodes { get; set; }

            public MediaStatus? status { get; set; }

            public int? meanScore { get; set; }

            public List<string> genres { get; set; }

            public int? seasonYear { get; set; }

            public FuzzyDate startDate { get; set; }

            public FuzzyDate endDate { get; set; }

            public List<Tag> tags { get; set; }

            public NodesStudios studios { get; set; }

            public List<ExternalLink> externalLinks { get; set; }

            public bool? isAdult { get; set; }
        }
    }
}
