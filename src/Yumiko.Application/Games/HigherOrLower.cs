using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

public static class HigherOrLower
{
    /// <summary>
    /// Picks two distinct random media from the list. Returns <c>null</c> if there are not at least two.
    /// </summary>
    public static (Anime First, Anime Second)? PickPair(IReadOnlyList<Anime> list, Random? random = null)
    {
        if (list.Count < 2)
        {
            return null;
        }

        Random rnd = random ?? Random.Shared;

        int first = rnd.Next(list.Count);
        int second;
        do
        {
            second = rnd.Next(list.Count);
        }
        while (first == second);

        return (list[first], list[second]);
    }

    /// <summary>
    /// Value compared in each mode: average score or amount of favourites.
    /// </summary>
    public static int ComparedValue(Anime anime, GamemodeHoL gamemode) => gamemode switch
    {
        GamemodeHoL.Score => anime.AvarageScore,
        GamemodeHoL.Popularity => anime.Favourites,
        _ => throw new ArgumentOutOfRangeException(nameof(gamemode)),
    };

    /// <summary>
    /// Whether the player choice is right. Ties count as a hit.
    /// </summary>
    public static bool IsCorrect(Anime selected, Anime other, GamemodeHoL gamemode) =>
        ComparedValue(selected, gamemode) >= ComparedValue(other, gamemode);

    /// <summary>AniList score on a 0-10 scale (it comes as 0-100).</summary>
    public static double ScoreOutOfTen(Anime anime) => anime.AvarageScore / 10d;
}
