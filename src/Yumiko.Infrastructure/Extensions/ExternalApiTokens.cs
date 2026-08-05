namespace Yumiko.Infrastructure.Extensions;

/// <summary>
/// Tokens of the external APIs. They are passed by parameter so Infrastructure never reads configuration.
/// </summary>
public sealed record ExternalApiTokens
{
    public required string OpenWeatherMap { get; init; }

    public required string TheCatApi { get; init; }

    public required string TheDogApi { get; init; }

    public string? Topgg { get; init; }
}
