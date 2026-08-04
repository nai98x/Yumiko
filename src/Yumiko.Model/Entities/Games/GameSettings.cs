namespace Yumiko.Model.Entities
{
    using Yumiko.Model.Enum;

    public class GameSettings
    {
        public bool Ok { get; set; }

        public string? ErrorMessage { get; set; }

        public int Rounds { get; set; }

        public int PageFrom { get; set; }

        public int PageTo { get; set; }

        public Gamemode Gamemode { get; set; }

        public Difficulty Difficulty { get; set; }

        public string? Genre { get; set; }
    }
}
