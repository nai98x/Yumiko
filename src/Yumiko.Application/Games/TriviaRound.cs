using Yumiko.Application.Helpers;

namespace Yumiko.Application.Games;

public static class TriviaRound
{
    /// <summary>Cantidad de opciones que se muestran por ronda: la correcta más cuatro señuelos.</summary>
    public const int OptionsPerRound = 5;

    /// <summary>
    /// Índices distintos dentro del pool para una ronda. El primero es la respuesta correcta.
    /// Devuelve menos de <see cref="OptionsPerRound"/> solo si el pool no da para más.
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
