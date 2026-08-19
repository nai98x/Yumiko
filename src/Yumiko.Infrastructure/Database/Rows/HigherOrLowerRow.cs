namespace Yumiko.Infrastructure.Database.Rows;

internal sealed class HigherOrLowerRow
{
    public long GuildId { get; set; }

    public long UserId { get; set; }

    public int Score { get; set; }
}
