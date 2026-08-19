namespace Yumiko.Infrastructure.Database.Rows;

internal sealed class QuizStatsRow
{
    public long GuildId { get; set; }

    public long UserId { get; set; }

    public string Gamemode { get; set; } = string.Empty;

    // The name of a Difficulty, or the name of a genre when Gamemode is "Genres".
    public string Difficulty { get; set; } = string.Empty;

    public int GamesPlayed { get; set; }

    public int CorrectRounds { get; set; }

    public int TotalRounds { get; set; }

    public int AccuracyPercentage { get; set; }
}
