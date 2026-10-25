namespace Yumiko.Application.Anilist;

public static class MediaTitles
{
    private const int MinSharedLength = 10;

    private const int MinNameWordLength = 3;

    /// <summary>
    /// Whether two romaji titles look like entries of the same series. Sequels, seasons, movies and specials
    /// usually extend the original title ("Shingeki no Kyojin" / "Shingeki no Kyojin Season 2") or share a long
    /// start with it ("Mushoku Tensei: ..." / "Mushoku Tensei II: ..."). The shared start is cut on a word
    /// boundary, so "K" does not swallow "Kanon" and "Boku no" alone is not enough. It leans towards false
    /// positives ("Shingeki no Bahamut"), which only cost a candidate.
    /// </summary>
    public static bool AreSameSeries(string? first, string? second)
    {
        string a = first?.Trim() ?? string.Empty;
        string b = second?.Trim() ?? string.Empty;

        if (a.Length == 0 || b.Length == 0)
        {
            return false;
        }

        int end = 0;

        while (end < a.Length && end < b.Length && char.ToUpperInvariant(a[end]) == char.ToUpperInvariant(b[end]))
        {
            end++;
        }

        if (!IsBoundary(a, end) || !IsBoundary(b, end))
        {
            while (end > 0 && char.IsLetterOrDigit(a[end - 1]))
            {
                end--;
            }
        }

        int shared = TrimmedLength(a, end);

        return shared > 0
            && (shared >= MinSharedLength || shared == TrimmedLength(a, a.Length) || shared == TrimmedLength(b, b.Length));
    }

    /// <summary>
    /// Whether a word of the character name shows up inside the title ("Violet Evergarden",
    /// "Code Geass: Hangyaku no Lelouch", "Horimiya" for Hori), which turns one into a giveaway of the other.
    /// Very short words are ignored so particles and initials do not count.
    /// </summary>
    public static bool ContainsName(string? title, string? name)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return SplitWords(name)
            .Where(word => word.Length >= MinNameWordLength)
            .Any(word => title.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> SplitWords(string text)
    {
        int start = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            if (i == text.Length || !char.IsLetterOrDigit(text[i]))
            {
                if (i > start)
                {
                    yield return text[start..i];
                }

                start = i + 1;
            }
        }
    }

    private static bool IsBoundary(string title, int index) =>
        index == title.Length || !char.IsLetterOrDigit(title[index]);

    private static int TrimmedLength(string title, int length)
    {
        while (length > 0 && !char.IsLetterOrDigit(title[length - 1]))
        {
            length--;
        }

        return length;
    }
}
