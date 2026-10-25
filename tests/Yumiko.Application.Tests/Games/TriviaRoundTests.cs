using Yumiko.Application.Games;

namespace Yumiko.Application.Tests.Games;

public class TriviaRoundTests
{
    [Fact]
    public void PickOptions_ReturnsTenDistinctIndexes()
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
    [InlineData(10)]
    public void PickOptions_NeverAsksForMoreOptionsThanThePoolHas(int size)
    {
        List<int> indices = TriviaRound.PickOptions(size);

        Assert.Equal(size, indices.Count);
        Assert.Equal(indices.Count, indices.Distinct().Count());
    }

    [Fact]
    public void PickOptions_DecoysShareTheGroupOfTheCorrectAnswer()
    {
        List<string?> groups = [.. Enumerable.Range(0, 60).Select(i => (i % 3) switch { 0 => "Male", 1 => "Female", _ => null })];

        for (int seed = 0; seed < 200; seed++)
        {
            List<int> indices = TriviaRound.PickOptions(groups, new Random(seed));
            string? group = groups[indices[0]];

            Assert.Equal(TriviaRound.OptionsPerRound, indices.Count);
            Assert.Equal(indices.Count, indices.Distinct().Count());

            if (group is not null)
            {
                Assert.All(indices, index => Assert.Equal(group, groups[index]));
            }
        }
    }

    [Fact]
    public void PickOptions_UngroupedAnswerTakesDecoysFromTheWholePool()
    {
        List<string?> groups = [null, .. Enumerable.Repeat<string?>("Male", 30), .. Enumerable.Repeat<string?>("Female", 30)];
        bool sawMale = false;
        bool sawFemale = false;

        for (int seed = 0; seed < 2000; seed++)
        {
            List<int> indices = TriviaRound.PickOptions(groups, new Random(seed));

            if (indices[0] != 0)
            {
                continue;
            }

            sawMale |= indices.Skip(1).Any(i => groups[i] == "Male");
            sawFemale |= indices.Skip(1).Any(i => groups[i] == "Female");
        }

        Assert.True(sawMale && sawFemale);
    }

    [Fact]
    public void PickOptions_FillsWithOtherGroupsWhenTheGroupRunsShort()
    {
        List<string?> groups = [.. Enumerable.Repeat<string?>("Male", 20), "Female", "Female", "Female"];

        for (int seed = 0; seed < 200; seed++)
        {
            List<int> indices = TriviaRound.PickOptions(groups, new Random(seed));

            Assert.Equal(TriviaRound.OptionsPerRound, indices.Count);
            Assert.Equal(indices.Count, indices.Distinct().Count());

            if (groups[indices[0]] == "Female")
            {
                Assert.All(indices.Skip(1).Take(2), index => Assert.Equal("Female", groups[index]));
            }
        }
    }

    [Fact]
    public void PickOptions_EmptyPoolReturnsNothing() =>
        Assert.Empty(TriviaRound.PickOptions([]));

    [Theory]
    [InlineData("Male", "Male")]
    [InlineData("male", "Male")]
    [InlineData("Female", "Female")]
    [InlineData("FEMALE", "Female")]
    [InlineData("Non-binary", null)]
    [InlineData("Robot", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void CharacterGenderGroup_OnlyGroupsMaleAndFemale(string? gender, string? expected) =>
        Assert.Equal(expected, TriviaRound.CharacterGenderGroup(gender));
}
