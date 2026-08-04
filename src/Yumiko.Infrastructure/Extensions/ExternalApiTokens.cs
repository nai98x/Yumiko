namespace Yumiko.Infrastructure.Extensions;

/// <summary>
/// Tokens de las APIs externas. Se pasan por parámetro para que Infrastructure nunca lea configuración.
/// </summary>
public sealed record ExternalApiTokens
{
    public required string OpenWeatherMap { get; init; }

    public required string TheCatApi { get; init; }

    public required string TheDogApi { get; init; }

    public string? Topgg { get; init; }
}
