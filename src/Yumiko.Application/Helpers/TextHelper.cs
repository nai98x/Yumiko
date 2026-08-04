using System.Globalization;
using System.Text.RegularExpressions;

namespace Yumiko.Application.Helpers;

public static partial class TextHelper
{
    // AniList devuelve HTML y su propio marcado de spoilers; esto lo lleva a markdown de Discord.
    public static string CleanText(string? text)
    {
        if (text is null)
        {
            return string.Empty;
        }

        text = text.Replace("<br>", string.Empty);
        text = text.Replace("<Br>", string.Empty);
        text = text.Replace("<bR>", string.Empty);
        text = text.Replace("<BR>", string.Empty);
        text = text.Replace("<i>", "*");
        text = text.Replace("<I>", "*");
        text = text.Replace("</i>", "*");
        text = text.Replace("</I>", "*");
        text = text.Replace("~!", "||");
        text = text.Replace("!~", "||");
        text = text.Replace("__", "**");
        text = text.Replace("<b>", "**");
        text = text.Replace("<B>", "**");
        text = text.Replace("</b>", "**");
        text = text.Replace("</B>", "**");

        return text;
    }

    public static string RemoveSpecialCharacters(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        return NonAlphanumeric().Replace(str, " ").Trim();
    }

    /// <summary>Title case sobre el texto completo llevado antes a minúsculas.</summary>
    public static string UppercaseFirst(this string? s) =>
        string.IsNullOrEmpty(s)
            ? string.Empty
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
