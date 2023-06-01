global using DSharpPlus;
global using DSharpPlus.Entities;
global using DSharpPlus.EventArgs;
global using DSharpPlus.Interactivity;
global using DSharpPlus.Interactivity.Enums;
global using DSharpPlus.Interactivity.Extensions;
global using DSharpPlus.SlashCommands;
global using DSharpPlus.SlashCommands.Attributes;
global using DSharpPlus.SlashCommands.EventArgs;
global using Microsoft.Extensions.Configuration;
global using System.Globalization;
global using Yumiko.Commands;
global using Yumiko.Datatypes;
global using Yumiko.Datatypes.Firebase;
global using Yumiko.Enums;
global using Yumiko.Providers;
global using Yumiko.Services;
global using Yumiko.Services.Firebase;
global using Yumiko.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Yumiko
{
    public partial class Program
    {
        internal static DiscordShardedClient DiscordShardedClient { get; set; } = null!;
        internal static ServiceProvider ServiceProvider { get; set; } = null!;
        internal static IConfigurationRoot Configuration { get; private set; } = null!;
        public static bool Debug { get; private set; }
        public static bool TopggEnabled { get; private set; }
        public static DiscordChannel LogChannelGuilds { get; private set; } = null!;
        public static DiscordChannel LogChannelErrors { get; private set; } = null!;
        public static Stopwatch Stopwatch { get; private set; } = null!;

        private static System.Timers.Timer cacheTimer = new System.Timers.Timer(24 * 60 * 60 * 1000);

        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            DebugMode();
            Console.Title = "Yumiko";

            cacheTimer.Elapsed += UpdateCacheAsync;
            cacheTimer.Enabled = true;

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile(Path.Join("res", "config.json"), true, true);
            configurationBuilder.AddCommandLine(args);
            Configuration = configurationBuilder.Build();
            services.AddSingleton(Configuration);

            services.AddLogging(loggingBuilder =>
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithProcessId()
                    .MinimumLevel.Is(Debug ? LogEventLevel.Debug : LogEventLevel.Information)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{ProcessId}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: $"logs/{DateTime.Now.ToString("dd'-'MM'-'yyyy' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log",
                        levelSwitch: new LoggingLevelSwitch(LogEventLevel.Information),
                        outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss}] [{Level:u4}]: {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 8_388_608, /* 8 megabytes */
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 50);

                Log.Logger = loggerConfiguration.CreateLogger().ForContext<Program>();

                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger, dispose: true);
            });

            ServiceProvider = services.BuildServiceProvider();

            if (!ConfigFilesCheck())
            {
                return;
            }

            DiscordShardedClient = new DiscordShardedClient(new()
            {
                Token = ConfigurationUtils.GetConfiguration<string>(Configuration, Debug ? Configurations.TokenDiscordTesting : Configurations.TokenDiscordProduction),
                Intents = DiscordIntents.Guilds,
                LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>(),
                ReconnectIndefinitely = true,
                MessageCacheSize = 0,
                AlwaysCacheMembers = false,
                LogUnknownEvents = false
            });

            await DiscordShardedClient.UseInteractivityAsync(new InteractivityConfiguration()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.DeleteMessage,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromSeconds(ConfigurationUtils.GetConfiguration<double>(Configuration, Enums.Configurations.TimeoutGeneral))
            });

            DiscordShardedClient.Ready += Client_Ready;
            DiscordShardedClient.Resumed += Client_Resumed;
            DiscordShardedClient.GuildDownloadCompleted += Client_GuildDownloadCompleted;
            DiscordShardedClient.GuildCreated += Client_GuildCreated;
            DiscordShardedClient.GuildDeleted += Client_GuildDeleted;
            DiscordShardedClient.ClientErrored += Client_ClientError;
            DiscordShardedClient.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            DiscordShardedClient.ModalSubmitted += Client_ModalSubmitted;

            await DiscordShardedClient.StartAsync();

            ulong logGuildId = ConfigurationUtils.GetConfiguration<ulong>(Configuration, Enums.Configurations.LogginGuildId);
            int shardCount = DiscordShardedClient.ShardClients.Count;
            int logGuildShard = ((int)logGuildId >> 22) % shardCount;

            var config = new SlashCommandsConfiguration()
            {
                Services = ServiceProvider
            };

            foreach (var keyValuePair in (await DiscordShardedClient.UseSlashCommandsAsync(config)))
            {
                var slashShardExtension = keyValuePair.Value;

                slashShardExtension.SlashCommandExecuted += SlashCommands_SlashCommandExecuted;
                slashShardExtension.SlashCommandErrored += SlashCommands_SlashCommandErrored;
                slashShardExtension.ContextMenuExecuted += SlashCommands_ContextMenuExecuted;
                slashShardExtension.ContextMenuErrored += SlashCommands_ContextMenuErrored;

                if (Debug && keyValuePair.Key == logGuildShard)
                {
                    slashShardExtension.RegisterCommands<Games>(logGuildId);
                    slashShardExtension.RegisterCommands<Stats>(logGuildId);
                    slashShardExtension.RegisterCommands<Interact>(logGuildId);
                    slashShardExtension.RegisterCommands<Anilist>(logGuildId);
                    slashShardExtension.RegisterCommands<Misc>(logGuildId);
                    slashShardExtension.RegisterCommands<Help>(logGuildId);
                }
                else
                {
                    slashShardExtension.RegisterCommands<Games>();
                    slashShardExtension.RegisterCommands<Stats>();
                    slashShardExtension.RegisterCommands<Interact>();
                    slashShardExtension.RegisterCommands<Anilist>();
                    slashShardExtension.RegisterCommands<Misc>();
                    slashShardExtension.RegisterCommands<Help>();
                }

                if (keyValuePair.Key == logGuildShard)
                {
                    slashShardExtension.RegisterCommands<Owner>(logGuildId);
                }
            }

            Stopwatch = Stopwatch.StartNew();

            await Task.Delay(-1);
        }

        internal static bool ConfigFilesCheck()
        {
            var configPath = Path.Join("res", "config.json");
            if (!File.Exists(configPath))
            {
                Log.Fatal($"{configPath} was not found!");
                return false;
            }

            var firebasePath = Path.Join("res", "firebase.json");
            if (!File.Exists(firebasePath))
            {
                Log.Fatal($"{firebasePath} was not found!");
                return false;
            }

            TopggEnabled = ConfigurationUtils.GetConfiguration<bool>(Configuration, Configurations.TopggEnabled);

            return true;
        }

        internal static void DebugMode()
        {
#if DEBUG
            Debug = true;
#else
            Debug = false;
#endif
        }

        private static void UpdateCacheAsync(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await Singleton.GetInstance().UpdateCachedMediaAsync();
                Log.Logger.Information("Media cache from AniList updated", DateTime.Now);
            });
        }
    }
}
