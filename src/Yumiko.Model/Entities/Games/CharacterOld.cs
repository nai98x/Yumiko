namespace Yumiko.Model.Entities
{
    public class CharacterOld
    {
        public string? NameFirst { get; set; }

        public string? NameLast { get; set; }

        public string? NameFull { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public string? SiteUrl { get; set; }

        public int? Favourites { get; set; }

        public Anime? MainAnime { get; set; }
    }
}
