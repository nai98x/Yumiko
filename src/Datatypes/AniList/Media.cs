namespace Yumiko.Datatypes
{
#pragma warning disable IDE1006 // Naming Styles
    public class Media
    {
        public int id { get; }
        public MediaTitle title { get; }
        public List<string> synonyms { get; }
        public string description { get; }
        public string siteUrl { get; }
        public MediaCoverImage coverImage { get; }
        public MediaFormat format { get; }
        public int chapters { get; }
        public int episodes { get; }
        public MediaStatus status { get; }
        public int meanScore { get; }
        public List<string> genres { get; }
        public int seasonYear { get; }
        public FuzzyDate startDate { get; }
        public FuzzyDate endDate { get; }
        public List<Tag> tags { get; }
        public NodesStudios studios { get; }
        public List<ExternalLink> externalLinks { get; }

    }
#pragma warning restore IDE1006 // Naming Styles
}
