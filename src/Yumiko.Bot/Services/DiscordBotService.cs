using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;

namespace Yumiko.Bot.Services;

public class DiscordBotService(DiscordClient client, BotConfiguration config, ILogger<DiscordBotService> logger)
    : IHostedService
{
    private readonly Stopwatch _uptime = new();
    private bool _connected;

    private DiscordChannel? _logChannelGuilds;
    private DiscordChannel? _logChannelErrors;

    public DiscordClient Client => client;

    public bool Debug => BotEnvironment.IsDebug;

    public bool Initialized { get; private set; }

    public bool InitializationFailed { get; private set; }

    public TimeSpan Uptime => _uptime.Elapsed;

    public DiscordChannel LogChannelGuilds =>
        _logChannelGuilds ?? throw new InvalidOperationException($"Canal {nameof(LogChannelGuilds)} no inicializado.");

    public DiscordChannel LogChannelErrors =>
        _logChannelErrors ?? throw new InvalidOperationException($"Canal {nameof(LogChannelErrors)} no inicializado.");

    public void SetChannels()
    {
        DiscordGuild guild = client.Guilds[config.LogGuildId];
        _logChannelGuilds = GetChannel(guild, config.Channels.Guilds, nameof(config.Channels.Guilds));
        _logChannelErrors = GetChannel(guild, config.Channels.Errors, nameof(config.Channels.Errors));
    }

    public void SetInitialized() => Initialized = true;

    public void SetInitializationFailed() => InitializationFailed = true;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando bot de Discord");
        _uptime.Start();
        await client.ConnectAsync();
        _connected = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _uptime.Stop();

        // Si nunca llegó a conectar (token inválido, Discord caído), DisconnectAsync tira un
        // NullReferenceException desde MultiShardOrchestrator y enmascara el error real.
        if (!_connected)
        {
            return;
        }

        logger.LogInformation("Desconectando bot de Discord");
        await client.DisconnectAsync();
    }

    private static DiscordChannel GetChannel(DiscordGuild guild, ulong id, string name) =>
        guild.Channels.TryGetValue(id, out DiscordChannel? channel)
            ? channel
            : throw new InvalidOperationException($"Canal {name} ({id}) no encontrado en el guild de logs.");
}
