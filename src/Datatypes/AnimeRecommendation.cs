namespace Yumiko.Datatypes
{
    public class AnimeRecommendation
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public decimal Score { get; set; } = 0;
    }
}
