namespace Yumiko.Model.Entities
{
    using Yumiko.Model.Enum;

    public class GameStatsUser
    {
        public Gamemode Gamemode { get; set; }

        public List<GameStats> Stats { get; set; } = new();
    }
}
