using System.Text;

namespace Yumiko.Application.Fun;

public static class LoveMeter
{
    private const int Segments = 20;

    /// <summary>
    /// 20 segment bar: each filled block represents 5%. The division is integer, so
    /// any percentage that is not a multiple of 5 rounds down.
    /// </summary>
    public static string Bar(int percentage)
    {
        int filled = percentage / 5;
        StringBuilder bar = new();

        for (int i = 0; i < filled; i++)
        {
            bar.Append('█');
        }

        for (int i = 0; i < Segments - filled; i++)
        {
            bar.Append(" . ");
        }

        return bar.ToString();
    }

    /// <summary>
    /// "Real" percentage: deterministic from the ids, so the same couple always gets the same result.
    /// The seed truncates to <c>int</c> on purpose; changing it would change the result of every couple.
    /// </summary>
    public static int RealPercentage(ulong id1, ulong id2) => new Random((int)(id1 + id2)).Next(0, 101);
}
