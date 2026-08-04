using Yumiko.Application.Helpers;

namespace Yumiko.Application.Tests.Helpers;

public class RandomHelperTests
{
    private static Random Seeded() => new(12345);

    [Fact]
    public void GetRandomNumber_BothNonPositive_ReturnsZero()
    {
        Assert.Equal(0, RandomHelper.GetRandomNumber(0, 0, Seeded()));
        Assert.Equal(0, RandomHelper.GetRandomNumber(-5, -1, Seeded()));
    }

    [Fact]
    public void GetRandomNumber_RangeOfTwo_ReturnsEitherEnd()
    {
        Random rnd = Seeded();
        for (int i = 0; i < 100; i++)
        {
            int value = RandomHelper.GetRandomNumber(3, 4, rnd);
            Assert.True(value is 3 or 4);
        }
    }

    [Fact]
    public void GetRandomNumber_MaximumIsInclusive()
    {
        Random rnd = Seeded();
        bool vioElMaximo = false;
        for (int i = 0; i < 500; i++)
        {
            int value = RandomHelper.GetRandomNumber(1, 5, rnd);
            Assert.InRange(value, 1, 5);
            vioElMaximo |= value == 5;
        }

        Assert.True(vioElMaximo);
    }

    [Fact]
    public void Shuffle_KeepsEveryElement()
    {
        List<int> list = [.. Enumerable.Range(0, 50)];

        RandomHelper.Shuffle(list, Seeded());

        Assert.Equal(50, list.Count);
        Assert.Equal(Enumerable.Range(0, 50), list.Order());
    }

    [Fact]
    public void Shuffle_SameSeedGivesTheSameOrder()
    {
        List<int> a = [.. Enumerable.Range(0, 20)];
        List<int> b = [.. Enumerable.Range(0, 20)];

        RandomHelper.Shuffle(a, Seeded());
        RandomHelper.Shuffle(b, Seeded());

        Assert.Equal(a, b);
    }

    [Fact]
    public void Swap_SwapsThePositions()
    {
        List<string> list = ["a", "b", "c"];

        RandomHelper.Swap(list, 0, 2);

        Assert.Equal(["c", "b", "a"], list);
    }
}
