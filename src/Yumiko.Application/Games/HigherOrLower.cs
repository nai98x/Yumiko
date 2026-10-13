using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

public static class HigherOrLower
{
    /// <summary>
    /// Picks two distinct random media from the list that form a valid pair (see <see cref="IsValidPair"/>).
    /// Returns <c>null</c> if there is no such pair.
    /// </summary>
    public static (Anime First, Anime Second)? PickPair(
        IReadOnlyList<Anime> list,
        GamemodeHoL gamemode,
        double minScoreDifference,
        Random? random = null)
    {
        List<Anime> firsts = [.. list.Where(a => list.Any(b => IsValidPair(a, b, gamemode, minScoreDifference)))];

        if (firsts.Count == 0)
        {
            return null;
        }

        Random rnd = random ?? Random.Shared;

        Anime first = firsts[rnd.Next(firsts.Count)];
        List<Anime> seconds = [.. list.Where(b => IsValidPair(first, b, gamemode, minScoreDifference))];

        return (first, seconds[rnd.Next(seconds.Count)]);
    }

    /// <summary>Whether the list still has at least one valid pair to play.</summary>
    public static bool HasPair(IReadOnlyList<Anime> list, GamemodeHoL gamemode, double minScoreDifference) =>
        list.Any(a => list.Any(b => IsValidPair(a, b, gamemode, minScoreDifference)));

    /// <summary>
    /// Two different media can face each other. In <see cref="GamemodeHoL.Score"/> their scores (0-10 scale)
    /// must differ by at least <paramref name="minScoreDifference"/>, so near ties are not asked.
    /// </summary>
    public static bool IsValidPair(Anime first, Anime second, GamemodeHoL gamemode, double minScoreDifference)
    {
        if (ReferenceEquals(first, second))
        {
            return false;
        }

        // AniList scores are integers on a 0-100 scale.
        return gamemode != GamemodeHoL.Score
            || Math.Abs(first.AvarageScore - second.AvarageScore) >= (int)Math.Round(minScoreDifference * 10);
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
