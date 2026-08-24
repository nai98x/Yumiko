using Yumiko.Application.Backups;

namespace Yumiko.Application.Tests.Backups;

public class BackupStateTests
{
    private static readonly DateOnly Today = new(2026, 8, 24);

    [Theory]
    [InlineData("2026-08-24")]
    [InlineData("  2026-08-24\n")]
    [InlineData("2026-08-25")]
    public void IsUpToDate_AcceptsTheMarkOfTodayOrLater(string mark)
    {
        Assert.True(BackupState.IsUpToDate(mark, Today));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2026-08-23")]
    [InlineData("24-08-2026")]
    [InlineData("not a date")]
    public void IsUpToDate_RejectsAnOldOrUnreadableMark(string? mark)
    {
        Assert.False(BackupState.IsUpToDate(mark, Today));
    }

    [Fact]
    public void DescribeLast_NormalizesTheDate()
    {
        Assert.Equal("2026-08-23", BackupState.DescribeLast("  2026-08-23\n"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("garbage")]
    public void DescribeLast_FallsBackWhenTheMarkIsNotReadable(string? mark)
    {
        Assert.Equal("unknown", BackupState.DescribeLast(mark));
    }
}
