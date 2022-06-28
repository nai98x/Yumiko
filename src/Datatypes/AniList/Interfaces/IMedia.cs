namespace Yumiko.Datatypes
{
#pragma warning disable IDE1006 // Naming Styles
    public interface IMedia
    {
        int id { get; }
        MediaTitle title { get; }
        List <string> synonyms { get; }
        string description { get; }
        string siteUrl { get; }
        MediaCoverImage coverImage { get; }
        MediaFormat? format { get; }
        int? volumes { get; }
        int? chapters { get; }
        int? episodes { get; }
        MediaStatus? status { get; }
        int? meanScore { get; }
        List <string> genres { get; }
        int? seasonYear { get; }
        FuzzyDate startDate { get; }
        FuzzyDate endDate { get; }
        List<Tag> tags { get; }
        NodesStudios studios { get; }
        List<ExternalLink> externalLinks { get; }
        bool isAdult { get; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
