using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

public class MediaPoolBuilderTests
{
    [Fact]
    public void PickPages_SinglePageRangeReturnsZero()
    {
        // La query interpreta la página 0 como la primera.
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
    [InlineData(1)]
    [InlineData(2)]
    public void GenreRange_FallsBackToTheFirstPageWhenTooFewPages(int lastPage)
    {
        Assert.Equal((0, 1), MediaPoolBuilder.GenreRange(lastPage));
    }

    [Fact]
    public void GenreRange_NeverGoesPastTheLastPage()
    {
        for (int i = 0; i < 200; i++)
        {
            (int pageFrom, int pageTo) = MediaPoolBuilder.GenreRange(4);

            Assert.Equal(0, pageFrom);
            Assert.InRange(pageTo, 1, 4);
        }
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
