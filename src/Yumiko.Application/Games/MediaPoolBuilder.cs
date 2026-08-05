using Yumiko.Application.Helpers;

namespace Yumiko.Application.Games;

public static class MediaPoolBuilder
{
    /// <summary>
    /// Picks the two pool pages to fetch. With a single page range it returns
    /// <c>(0, 0)</c>: page 0 is what the query reads as "the first one".
    /// </summary>
    public static (int First, int Second) PickPages(int pageFrom, int pageTo, Random? random = null)
    {
        if (pageFrom == pageTo)
        {
            return (0, 0);
        }

        int first = RandomHelper.GetRandomNumber(pageFrom, pageTo, random);
        int second;

        do
        {
            second = RandomHelper.GetRandomNumber(pageFrom, pageTo, random);
        }
        while (second == first);

        return (first, second);
    }

    /// <summary>
    /// Page range to draw from in genres mode, based on the last page AniList reported.
    /// With less than 3 pages there is nothing to choose from and the first one is always used.
    /// </summary>
    public static (int PageFrom, int PageTo) GenreRange(int lastPage, Random? random = null) =>
        lastPage < 3 ? (0, 1) : (0, Math.Min(RandomHelper.GetRandomNumber(1, 9, random), lastPage));

    /// <summary>Page range associated with each trivia difficulty.</summary>
    public static (int PageFrom, int PageTo) DifficultyRange(Model.Enum.Difficulty difficulty) => difficulty switch
    {
        Model.Enum.Difficulty.Easy => (1, 10),
        Model.Enum.Difficulty.Hard => (30, 60),
        Model.Enum.Difficulty.Extreme => (60, 100),
        _ => (10, 30),
    };
}
