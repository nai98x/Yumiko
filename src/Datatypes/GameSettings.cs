namespace Yumiko.Datatypes
{
    public class GameSettings
    {
        public bool Ok { get; set; }

        public string? MsgError { get; set; }

        public int Rondas { get; set; }

        public int IterIni { get; set; }

        public int IterFin { get; set; }

        public Gamemode Gamemode { get; set; }

        public Difficulty Difficulty { get; set; }

        public string? Genre { get; set; }

        public string? Studio { get; set; }
    }
}
