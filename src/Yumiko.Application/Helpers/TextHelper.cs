using System.Globalization;
using System.Text.RegularExpressions;

namespace Yumiko.Application.Helpers;

public static partial class TextHelper
{
    // AniList returns HTML and its own spoiler markup; this converts it to Discord markdown.
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

    /// <summary>Title case over the whole text lowercased beforehand.</summary>
    public static string UppercaseFirst(this string? s) =>
        string.IsNullOrEmpty(s)
            ? string.Empty
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
