namespace Yumiko.Model.Entities
{
    using System.Collections.Generic;

    public class Anime
    {
        public int Id { get; set; }

        public string TitleRomaji { get; set; } = null!;

        public string? TitleEnglish { get; set; }

        public string? TitleRomajiFormatted { get; set; }

        public string? TitleEnglishFormatted { get; set; }

        public string? Image { get; set; }

        public string? SiteUrl { get; set; }

        public int AvarageScore { get; set; }

        public int Favourites { get; set; }

        public List<string>? Synonyms { get; set; }

        public int Popularity { get; set; }

        public List<StudioOld>? Studios { get; set; }

        public List<CharacterOld>? Characters { get; set; }

        public string? Relation { get; set; }

        public string? Type { get; set; }

        public List<Anime>? RelatedMedia { get; set; }
    }
}
