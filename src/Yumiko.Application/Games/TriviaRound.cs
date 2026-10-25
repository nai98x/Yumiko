using Yumiko.Application.Helpers;

namespace Yumiko.Application.Games;

public static class TriviaRound
{
    /// <summary>Amount of options shown per round: the correct one plus nine decoys.</summary>
    public const int OptionsPerRound = 10;

    private const string Male = "Male";
    private const string Female = "Female";

    /// <summary>
    /// Distinct indexes inside the pool for one round. The first one is the correct answer.
    /// Returns fewer than <see cref="OptionsPerRound"/> only if the pool cannot give more.
    /// </summary>
    public static List<int> PickOptions(int poolSize, Random? random = null) =>
        PickOptions(new string?[poolSize], random);

    /// <summary>
    /// Like <see cref="PickOptions(int, Random?)"/>, but the decoys come from the same group as the correct
    /// answer. A <c>null</c> group has no restriction. When the group runs short, the rest of the pool fills in.
    /// </summary>
    public static List<int> PickOptions(IReadOnlyList<string?> groups, Random? random = null)
    {
        Random rnd = random ?? Random.Shared;
        int count = Math.Min(OptionsPerRound, groups.Count);

        if (count == 0)
        {
            return [];
        }

        int correct = rnd.Next(groups.Count);
        string? group = groups[correct];

        List<int> sameGroup = [];
        List<int> others = [];

        for (int i = 0; i < groups.Count; i++)
        {
            if (i == correct)
            {
                continue;
            }

            if (group is null || string.Equals(groups[i], group, StringComparison.Ordinal))
            {
                sameGroup.Add(i);
            }
            else
            {
                others.Add(i);
            }
        }

        RandomHelper.Shuffle(sameGroup, rnd);
        RandomHelper.Shuffle(others, rnd);

        return [correct, .. sameGroup.Concat(others).Take(count - 1)];
    }

    /// <summary>
    /// Decoy group of a character by its AniList gender, which is free text. Only male and female characters
    /// are grouped; any other value (or none) can be matched against the whole pool.
    /// </summary>
    public static string? CharacterGenderGroup(string? gender) =>
        string.Equals(gender, Male, StringComparison.OrdinalIgnoreCase) ? Male
        : string.Equals(gender, Female, StringComparison.OrdinalIgnoreCase) ? Female
        : null;
}
