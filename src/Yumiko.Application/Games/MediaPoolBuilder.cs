using Yumiko.Application.Helpers;

namespace Yumiko.Application.Games;

public static class MediaPoolBuilder
{
    /// <summary>
    /// Elige las dos páginas del pool que se van a traer. Con un rango de una sola página devuelve
    /// <c>(0, 0)</c>: la página 0 es lo que la query interpreta como "la primera".
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
    /// Rango de páginas a sortear en el modo géneros, a partir de la última página que reportó AniList.
    /// Con menos de 3 páginas no hay de dónde elegir y se usa siempre la primera.
    /// </summary>
    public static (int PageFrom, int PageTo) GenreRange(int lastPage, Random? random = null) =>
        lastPage < 3 ? (0, 1) : (0, Math.Min(RandomHelper.GetRandomNumber(1, 9, random), lastPage));

    /// <summary>Rango de páginas asociado a cada dificultad de la trivia.</summary>
    public static (int PageFrom, int PageTo) DifficultyRange(Model.Enum.Difficulty difficulty) => difficulty switch
    {
        Model.Enum.Difficulty.Easy => (1, 10),
        Model.Enum.Difficulty.Hard => (30, 60),
        Model.Enum.Difficulty.Extreme => (60, 100),
        _ => (10, 30),
    };
}
