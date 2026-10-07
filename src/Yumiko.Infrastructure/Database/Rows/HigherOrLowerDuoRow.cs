namespace Yumiko.Infrastructure.Database.Rows;

internal sealed class HigherOrLowerDuoRow
{
    public long GuildId { get; set; }

    public long FirstUserId { get; set; }

    public long SecondUserId { get; set; }

    public int Score { get; set; }
}
