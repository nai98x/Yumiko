using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;

namespace Yumiko.Bot.Services;

/// <summary>
/// Stops the process when the gateway goes silent, so systemd restarts it. DSharpPlus can give up on
/// a shard after a failed resume (e.g. Discord answering 503) and leave the process alive but
/// disconnected, and the nightly never raises <c>Zombied</c>.
/// </summary>
/// <remarks>
/// The received events counter includes the heartbeat ACKs (every ~41 seconds per shard), so it only
/// stalls when no shard is receiving anything. It is global: a single dead shard among several
/// healthy ones goes unnoticed.
/// </remarks>
public sealed class GatewayWatchdogService(
    DiscordClient client,
    GatewaySettings settings,
    IHostApplicationLifetime lifetime,
    ILogger<GatewayWatchdogService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ulong lastCount = client.GetGatewayMetrics().EventsReceived;
        DateTimeOffset lastActivity = DateTimeOffset.UtcNow;

        using PeriodicTimer timer = new(settings.WatchdogCheckInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                ulong count = client.GetGatewayMetrics().EventsReceived;
                if (count != lastCount)
                {
                    lastCount = count;
                    lastActivity = DateTimeOffset.UtcNow;
                    continue;
                }

                TimeSpan silence = DateTimeOffset.UtcNow - lastActivity;
                if (silence < settings.WatchdogSilenceTimeout)
                {
                    continue;
                }

                logger.LogCritical(
                    "No gateway events received for {Silence}; stopping the process so it gets restarted",
                    silence);
                Environment.ExitCode = 1;
                lifetime.StopApplication();
                return;
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
