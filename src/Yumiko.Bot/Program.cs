using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Yumiko.Application.Extensions;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Infrastructure.Extensions;

namespace Yumiko.Bot;

public static class Program
{
    private static async Task Main(string[] args)
    {
        LogsSettings logsSettings = LogsSettings.FromConfiguration(new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build());

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(BotEnvironment.IsDebug ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.WithProcessId()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                theme: SystemConsoleTheme.Colored,
                outputTemplate: "[{Timestamp:HH:mm:ss}] [{ProcessId}] [{Level:u3}]: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                levelSwitch: new LoggingLevelSwitch(LogEventLevel.Information),
                path: "logs/yumiko-.log",
                outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{Level:u3}]: {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: logsSettings.FileSizeBytes,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: logsSettings.RetainedFileCount)
            .CreateLogger();

        try
        {
            IHost host = CreateHost(args);
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The main service terminated unexpectedly");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHost CreateHost(string[] args)
    {
        HostApplicationBuilder host = Host.CreateApplicationBuilder(args);

        host.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets(typeof(Program).Assembly, optional: true)
            .AddEnvironmentVariables();

        // The development overrides only apply on Debug builds. If they were always loaded, the
        // top.gg gating of that file would override the production one.
        if (BotEnvironment.IsDebug)
        {
            host.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
        }

        BotConfiguration botConfig = BotConfiguration.FromConfiguration(host.Configuration);
        TimeoutSettings timeouts = TimeoutSettings.FromConfiguration(host.Configuration);

        string discordToken = Required(host.Configuration, "discordToken");
        string firebaseCredentialsDir = Required(host.Configuration, "FIREBASE_CREDENTIALS_DIR");

        host.Services
            .AddSingleton(botConfig)
            .AddBehaviorSettings(host.Configuration)
            .AddApplication()
            .AddInfrastructure(firebaseCredentialsDir, new ExternalApiTokens
            {
                OpenWeatherMap = Required(host.Configuration, "openWeatherMapToken"),
                TheCatApi = Required(host.Configuration, "theCatApiToken"),
                TheDogApi = Required(host.Configuration, "theDogApiToken"),
                Topgg = host.Configuration.GetValue<string>("topggToken"),
            })
            .AddConfiguredDiscordClient(discordToken, botConfig, timeouts)
            .AddSerilog()
            .AddBotServices();

        return host.Build();
    }

    private static string Required(IConfiguration configuration, string key) =>
        configuration.GetValue<string>(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"'{key}' is required: set it via User Secrets (local) or an environment variable (server)");
}
