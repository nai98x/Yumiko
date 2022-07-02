namespace Yumiko.Datatypes
{
    public class GameStatsUser
    {
        public Gamemode Gamemode { get; set; }

        public List<GameStats> Stats { get; set; } = new();
    }
}
