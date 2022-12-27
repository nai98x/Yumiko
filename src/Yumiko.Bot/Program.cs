using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Rest.Core;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Globalization;
using Remora.Discord.Interactivity.Extensions;
using Yumiko.Bot.Commands;

namespace Remora.Discord.Samples.SlashCommands;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(
                configuration =>
                {
                    configuration.AddCommandLine(args);
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    configuration.AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);
                })
            .AddDiscordService
            (
                services =>
                {
                    var configuration = services.GetRequiredService<IConfiguration>();

                    return configuration.GetValue<string>("tokens:discord") ??
                           throw new InvalidOperationException
                           (
                               "No bot token has been provided."
                           );
                }
            )
            .ConfigureServices
            (
                (_, services) =>
                {
                    services.Configure<DiscordGatewayClientOptions>(g => g.Intents |= GatewayIntents.Guilds);
                    services
                        .AddDiscordCommands(true)
                        .AddCommandTree()
                            .WithCommandGroup<Interact>()
                            .Finish()
                        .AddInteractivity();
                }
            )
            .UseSerilog((ctx, lc) => lc
                .Enrich.FromLogContext()
                    .Enrich.WithProcessId()
                    .MinimumLevel.Is(IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{ProcessId}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: $"logs/{DateTime.Now.ToString("dd'-'MM'-'yyyy' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log",
                        levelSwitch: new LoggingLevelSwitch(LogEventLevel.Information),
                        outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 8_388_608, /* 8 megabytes */
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 100)
            )
            .UseConsoleLifetime()
            .Build();

        var services = host.Services;
        var log = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        Snowflake? debugServer = null;

        var debugServerString = configuration.GetValue<string?>("logging:guild_id");
        if (debugServerString is not null)
        {
            if (!DiscordSnowflake.TryParse(debugServerString, out debugServer))
            {
                log.LogWarning("Failed to parse debug server from environment");
            }
        }

        var slashService = services.GetRequiredService<SlashService>();

        if (!IsProduction()) /* Always update commands for debugging */
        {
            var updateSlash = await slashService.UpdateSlashCommandsAsync(debugServer);
            if (updateSlash.IsSuccess)
                log.LogInformation("Slash commands successfully updated in guild {debug}", debugServer);
            else
                log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
        }

        await host.RunAsync();
    }

    // Move this to another place
    public static bool IsProduction()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}