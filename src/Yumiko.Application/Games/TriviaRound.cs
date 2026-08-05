using Yumiko.Application.Helpers;

namespace Yumiko.Application.Games;

public static class TriviaRound
{
    /// <summary>Amount of options shown per round: the correct one plus four decoys.</summary>
    public const int OptionsPerRound = 5;

    /// <summary>
    /// Distinct indexes inside the pool for one round. The first one is the correct answer.
    /// Returns fewer than <see cref="OptionsPerRound"/> only if the pool cannot give more.
    /// </summary>
    public static List<int> PickOptions(int poolSize, Random? random = null)
    {
        int count = Math.Min(OptionsPerRound, poolSize);
        HashSet<int> chosen = [];
        List<int> indices = [];

        while (indices.Count < count)
        {
            int index = RandomHelper.GetRandomNumber(0, poolSize - 1, random);

            if (chosen.Add(index))
            {
                indices.Add(index);
            }
        }

        return indices;
    }
}
