using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Yumiko.Bot.Events.Handlers;

public sealed class SessionCreatedHandler(ILogger<SessionCreatedHandler> logger)
{
    public async Task Handle(DiscordClient client, SessionCreatedEventArgs args)
    {
        logger.LogInformation("Sesión creada; el cliente ya procesa eventos");
        await client.UpdateStatusAsync(
            new DiscordActivity { ActivityType = DiscordActivityType.ListeningTo, Name = "/help" },
            DiscordUserStatus.Online);
    }
}

public sealed class SessionResumedHandler(ILogger<SessionResumedHandler> logger)
{
    public Task Handle(DiscordClient client, SessionResumedEventArgs args)
    {
        logger.LogInformation("Sesión reanudada");
        return Task.CompletedTask;
    }
}

public sealed class ZombiedHandler(ILogger<ZombiedHandler> logger)
{
    public Task Handle(DiscordClient client, ZombiedEventArgs args)
    {
        logger.LogWarning("Conexión zombie detectada tras {Failures} heartbeats fallidos", args.Failures);
        return Task.CompletedTask;
    }
}
