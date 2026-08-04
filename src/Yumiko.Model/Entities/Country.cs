namespace Yumiko.Model.Entities;

public sealed class Country
{
    public required string NameEnglish { get; init; }

    public required string NameSpanish { get; init; }

    /// <summary>Código ISO de dos letras; es lo que espera OpenWeatherMap.</summary>
    public required string Code { get; init; }

    public string? DialCode { get; init; }

    public bool Matches(string? text) =>
        string.IsNullOrWhiteSpace(text)
        || NameEnglish.Contains(text, StringComparison.OrdinalIgnoreCase)
        || NameSpanish.Contains(text, StringComparison.OrdinalIgnoreCase);
}
