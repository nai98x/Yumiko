using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Yumiko.Bot.Configuration;

/// <summary>Interactivity timeouts, in seconds.</summary>
public sealed record TimeoutSettings(double General, double Games)
{
    public static TimeoutSettings FromConfiguration(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Timeouts");
        return new TimeoutSettings(
            RequireDouble(section, "General"),
            RequireDouble(section, "Games"));
    }

    private static double RequireDouble(IConfigurationSection section, string key) =>
        double.TryParse(section[key], NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : throw new InvalidOperationException($"Missing or invalid configuration in appsettings.json: {section.Path}:{key}");
}

public sealed record LogsSettings(long FileSizeBytes, int RetainedFileCount)
{
    public static LogsSettings FromConfiguration(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Logs");
        return new LogsSettings(
            section.GetValue<long?>("FileSizeBytes") ?? 8_388_608,
            section.GetValue<int?>("RetainedFileCount") ?? 50);
    }
}

public sealed record TopggSettings(bool Enabled)
{
    public static TopggSettings FromConfiguration(IConfiguration configuration) =>
        new(configuration.GetSection("Topgg").GetValue<bool>("Enabled"));
}

/// <summary>Parameters of the media pool that feeds the games.</summary>
public sealed record GamesSettings(int MediaCachePageFrom, int MediaCachePageTo, int AnilistPerPage, int RandomPageMax)
{
    public static GamesSettings FromConfiguration(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection("Games");
        return new GamesSettings(
            section.GetValue<int?>("MediaCachePageFrom") ?? 1,
            section.GetValue<int?>("MediaCachePageTo") ?? 36,
            section.GetValue<int?>("AnilistPerPage") ?? 25,
            // AniList rejects the query when page * perPage goes over 5000 entries.
            section.GetValue<int?>("RandomPageMax") ?? 5000);
    }
}

public static class BehaviorSettingsExtensions
{
    public static IServiceCollection AddBehaviorSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeoutSettings.FromConfiguration(configuration));
        services.AddSingleton(LogsSettings.FromConfiguration(configuration));
        services.AddSingleton(TopggSettings.FromConfiguration(configuration));
        services.AddSingleton(GamesSettings.FromConfiguration(configuration));
        return services;
    }
}
