namespace Yumiko.Model.Entities
{
    public class GameStats
    {
        public long UserId { get; set; }

        public int AccuracyPercentage { get; set; }

        public int GamesPlayed { get; set; }

        public int CorrectRounds { get; set; }

        public int TotalRounds { get; set; }

        public string? DifficultyName { get; set; }
    }
}
