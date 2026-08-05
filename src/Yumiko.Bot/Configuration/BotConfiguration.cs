using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Yumiko.Bot.Configuration;

/// <summary>
/// Ids and URLs the bot starts with. It fails at startup if any is missing, instead of blowing up
/// when someone uses the command.
/// </summary>
/// <remarks>
/// The Discord ids and <c>Website</c> come from appsettings.json;
/// <see cref="AnilistApiClientId"/> is a secret (User Secrets locally, environment variable on the
/// server) and that is why it is not versioned.
/// </remarks>
public sealed class BotConfiguration
{
    public required ulong LogGuildId { get; init; }

    public required ChannelConfiguration Channels { get; init; }

    public required string Website { get; init; }

    /// <summary>Client id of the AniList app, to build the OAuth URL of <c>/anilist setprofile</c>.</summary>
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
            : throw new InvalidOperationException($"Missing or invalid configuration in appsettings.json: {section.Path}:{key}");

    private static string RequireString(IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing or invalid configuration in appsettings.json: {key}");

    private static string RequireSecret(IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"'{key}' is required: set it via User Secrets (local) or an environment variable (server)");
}

public sealed class ChannelConfiguration
{
    /// <summary>Channel where guild joins and leaves are announced.</summary>
    public required ulong Guilds { get; init; }

    /// <summary>Channel where the errors are logged.</summary>
    public required ulong Errors { get; init; }
}
