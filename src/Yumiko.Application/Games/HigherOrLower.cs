using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

public static class HigherOrLower
{
    /// <summary>
    /// Elige dos medias distintos al azar de la lista. Devuelve <c>null</c> si no hay al menos dos.
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
    /// Valor que se compara en cada modo: puntaje promedio o cantidad de favoritos.
    /// </summary>
    public static int ComparedValue(Anime anime, GamemodeHoL gamemode) => gamemode switch
    {
        GamemodeHoL.Score => anime.AvarageScore,
        GamemodeHoL.Popularity => anime.Favourites,
        _ => throw new ArgumentOutOfRangeException(nameof(gamemode)),
    };

    /// <summary>
    /// Si la elección del jugador acierta. Los empates cuentan como acierto.
    /// </summary>
    public static bool IsCorrect(Anime selected, Anime other, GamemodeHoL gamemode) =>
        ComparedValue(selected, gamemode) >= ComparedValue(other, gamemode);

    /// <summary>Puntaje de AniList en escala 0-10 (viene en 0-100).</summary>
    public static double ScoreOutOfTen(Anime anime) => anime.AvarageScore / 10d;
}
