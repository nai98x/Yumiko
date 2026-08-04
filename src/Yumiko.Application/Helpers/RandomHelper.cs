namespace Yumiko.Application.Helpers;

public static class RandomHelper
{
    public static int GetRandomNumber(int min, int max, Random? random = null)
    {
        Random rnd = random ?? Random.Shared;

        if (min <= 0 && max <= 0)
        {
            return 0;
        }

        if (min + 1 == max)
        {
            return rnd.Next(100) < 50 ? min : max;
        }

        return rnd.Next(minValue: min, maxValue: max + 1);
    }

    public static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count; i > 0; i--)
        {
            Swap(list, 0, random.Next(0, i));
        }
    }

    public static void Swap<T>(IList<T> list, int i, int j) => (list[j], list[i]) = (list[i], list[j]);
}
