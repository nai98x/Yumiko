namespace Yumiko.Bot.Helpers;

/// <summary>
/// Recortes a los límites duros de Discord. Son constantes de la API, no configuración.
/// </summary>
public static class DiscordStringExtensions
{
    public static string NormalizeField(this string s) => Truncate(s, 1024);

    public static string NormalizeDescription(this string s) => Truncate(s, 4096);

    public static string NormalizeButton(this string s) => TruncateSimple(s, 80);

    public static string NormalizeCustomId(this string s) => TruncateSimple(s, 80);

    public static string NormalizeSelectMenuOption(this string s) => TruncateSimple(s, 100);

    public static string NormalizeDescriptionNewLine(this string s)
    {
        if (s.Length <= 4096)
        {
            return s;
        }

        string aux = s.Remove(4096);
        int newLine = aux.LastIndexOf('\n');
        return newLine == -1 ? aux : aux.Remove(newLine);
    }

    public static MemoryStream ToMemoryStream(this byte[] byteArray) => new(byteArray) { Position = 0 };

    /// <summary>Corta sin partir un enlace markdown por la mitad.</summary>
    private static string Truncate(string s, int limit)
    {
        if (s.Length <= limit)
        {
            return s;
        }

        string aux = s.Remove(limit);
        int bracket = aux.LastIndexOf('[');

        return bracket != -1 ? aux.Remove(bracket) + "..." : aux.Remove(aux.Length - 4) + " ...";
    }

    private static string TruncateSimple(string s, int limit) =>
        s.Length <= limit ? s : s.Remove(limit - 4) + " ...";
}
