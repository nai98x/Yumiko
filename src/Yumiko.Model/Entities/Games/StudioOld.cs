namespace Yumiko.Model.Entities
{
    public class StudioOld
    {
        public string Name { get; set; } = null!;

        public string? SiteUrl { get; set; }

        public int Favourites { get; set; }

        /// <summary>Most popular anime of the studio, most popular first.</summary>
        public List<Anime> PopularMedia { get; set; } = [];
    }
}
