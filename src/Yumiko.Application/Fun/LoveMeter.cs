using System.Text;

namespace Yumiko.Application.Fun;

public static class LoveMeter
{
    private const int Segments = 20;

    /// <summary>
    /// Barra de 20 segmentos: cada bloque lleno representa 5%. La división es entera, así que
    /// cualquier porcentaje que no sea múltiplo de 5 redondea hacia abajo.
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
    /// Porcentaje "real": determinista a partir de los ids, así siempre da lo mismo para la misma pareja.
    /// La semilla trunca a <c>int</c> a propósito; cambiarla cambiaría el resultado de todas las parejas.
    /// </summary>
    public static int RealPercentage(ulong id1, ulong id2) => new Random((int)(id1 + id2)).Next(0, 101);
}
