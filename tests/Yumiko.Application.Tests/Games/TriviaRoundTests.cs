using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaRoundTests
{
    [Fact]
    public void PickOptions_ReturnsFiveDistinctIndexes()
    {
        for (int i = 0; i < 200; i++)
        {
            List<int> indices = TriviaRound.PickOptions(50);

            Assert.Equal(TriviaRound.OptionsPerRound, indices.Count);
            Assert.Equal(indices.Count, indices.Distinct().Count());
            Assert.All(indices, index => Assert.InRange(index, 0, 49));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void PickOptions_NeverAsksForMoreOptionsThanThePoolHas(int size)
    {
        List<int> indices = TriviaRound.PickOptions(size);

        Assert.Equal(size, indices.Count);
        Assert.Equal(indices.Count, indices.Distinct().Count());
    }
}
