using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

public class MediaPoolBuilderTests
{
    [Fact]
    public void PickPages_SinglePageRangeReturnsZero()
    {
        // The query reads page 0 as the first one.
        Assert.Equal((0, 0), MediaPoolBuilder.PickPages(7, 7));
    }

    [Fact]
    public void PickPages_ReturnsTwoDistinctPagesInRange()
    {
        for (int i = 0; i < 200; i++)
        {
            (int first, int second) = MediaPoolBuilder.PickPages(10, 30);

            Assert.NotEqual(first, second);
            Assert.InRange(first, 10, 30);
            Assert.InRange(second, 10, 30);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void GenreRange_UsesTheFirstPageWhenThereIsOnlyOne(int lastPage)
    {
        Assert.Equal((1, 1), MediaPoolBuilder.GenreRange(lastPage, 20));
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(20, 20)]
    [InlineData(150, 20)]
    public void GenreRange_GoesUpToTheLastPageOrTheMaxPage(int lastPage, int expectedPageTo)
    {
        Assert.Equal((1, expectedPageTo), MediaPoolBuilder.GenreRange(lastPage, 20));
    }

    [Theory]
    [InlineData(Difficulty.Easy, 1, 10)]
    [InlineData(Difficulty.Normal, 10, 30)]
    [InlineData(Difficulty.Hard, 30, 60)]
    [InlineData(Difficulty.Extreme, 60, 100)]
    public void DifficultyRange_MapsEveryDifficulty(Difficulty difficulty, int pageFrom, int pageTo)
    {
        Assert.Equal((pageFrom, pageTo), MediaPoolBuilder.DifficultyRange(difficulty));
    }
}
