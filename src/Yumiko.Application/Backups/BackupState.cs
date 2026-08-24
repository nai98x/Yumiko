using System.Globalization;

namespace Yumiko.Application.Backups;

/// <summary>
/// Reads the mark the backup script writes after a successful upload: a single <c>yyyy-MM-dd</c> line.
/// </summary>
public static class BackupState
{
    public static bool IsUpToDate(string? mark, DateOnly today) =>
        TryParseMark(mark, out DateOnly date) && date >= today;

    public static string DescribeLast(string? mark) =>
        TryParseMark(mark, out DateOnly date) ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "unknown";

    private static bool TryParseMark(string? mark, out DateOnly date) =>
        DateOnly.TryParseExact(mark?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
