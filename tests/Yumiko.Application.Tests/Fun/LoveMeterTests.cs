using Yumiko.Application.Fun;

namespace Yumiko.Application.Tests.Fun;

public class LoveMeterTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 1)]
    [InlineData(49, 9)]
    [InlineData(50, 10)]
    [InlineData(99, 19)]
    [InlineData(100, 20)]
    public void Bar_FillsOneBlockEveryFivePercent(int percentage, int expectedBlocks)
    {
        string bar = LoveMeter.Bar(percentage);

        Assert.Equal(expectedBlocks, bar.Count(c => c == '█'));
        Assert.Equal(20 - expectedBlocks, bar.Split(" . ").Length - 1);
    }

    [Fact]
    public void RealPercentage_IsDeterministicAndSymmetric()
    {
        const ulong id1 = 268654073826312192;
        const ulong id2 = 132819036282322944;

        int first = LoveMeter.RealPercentage(id1, id2);

        Assert.Equal(first, LoveMeter.RealPercentage(id1, id2));
        Assert.Equal(first, LoveMeter.RealPercentage(id2, id1));
        Assert.InRange(first, 0, 100);
    }
}
