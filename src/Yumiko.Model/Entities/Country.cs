namespace Yumiko.Model.Entities;

public sealed class Country
{
    public required string NameEnglish { get; init; }

    public required string NameSpanish { get; init; }

    /// <summary>Two letter ISO code; that is what OpenWeatherMap expects.</summary>
    public required string Code { get; init; }

    public string? DialCode { get; init; }

    public bool Matches(string? text) =>
        string.IsNullOrWhiteSpace(text)
        || NameEnglish.Contains(text, StringComparison.OrdinalIgnoreCase)
        || NameSpanish.Contains(text, StringComparison.OrdinalIgnoreCase);
}
