using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Yumiko.Bot.Configuration;

/// <summary>
/// Ids y URLs con los que arranca el bot. Falla al arrancar si falta alguno, en vez de reventar
/// cuando alguien usa el comando.
/// </summary>
/// <remarks>
/// Los ids de Discord y <c>Website</c> salen de appsettings.json;
/// <see cref="AnilistApiClientId"/> es un secret (User Secrets en local, variable de entorno en el
/// servidor) y por eso no está versionado.
/// </remarks>
public sealed class BotConfiguration
{
    public required ulong LogGuildId { get; init; }

    public required ChannelConfiguration Channels { get; init; }

    public required string Website { get; init; }

    /// <summary>Client id de la app de AniList, para armar la URL de OAuth de <c>/anilist setprofile</c>.</summary>
    public required string AnilistApiClientId { get; init; }

    public static BotConfiguration FromConfiguration(IConfiguration configuration)
    {
        IConfigurationSection ids = configuration.GetSection("Ids");

        return new BotConfiguration
        {
            LogGuildId = RequireUlong(ids, "LogGuildId"),
            Channels = new ChannelConfiguration
            {
                Guilds = RequireUlong(ids.GetSection("Channels"), "Guilds"),
                Errors = RequireUlong(ids.GetSection("Channels"), "Errors"),
            },
            Website = RequireString(configuration, "Website"),
            AnilistApiClientId = RequireSecret(configuration, "AnilistApiClientId"),
        };
    }

    private static ulong RequireUlong(IConfigurationSection section, string key) =>
        ulong.TryParse(section[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong value)
            ? value
            : throw new InvalidOperationException($"Configuración faltante o inválida en appsettings.json: {section.Path}:{key}");

    private static string RequireString(IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Configuración faltante o inválida en appsettings.json: {key}");

    private static string RequireSecret(IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"'{key}' es obligatorio: configuralo via User Secrets (local) o variable de entorno (servidor)");
}

public sealed class ChannelConfiguration
{
    /// <summary>Canal donde se avisan altas y bajas de guild.</summary>
    public required ulong Guilds { get; init; }

    /// <summary>Canal donde se loguean los errores.</summary>
    public required ulong Errors { get; init; }
}
