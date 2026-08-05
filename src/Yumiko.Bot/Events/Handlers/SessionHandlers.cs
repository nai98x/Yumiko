using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Yumiko.Bot.Events.Handlers;

public sealed class SessionCreatedHandler(ILogger<SessionCreatedHandler> logger)
{
    public async Task Handle(DiscordClient client, SessionCreatedEventArgs args)
    {
        logger.LogInformation("Session created; the client is already processing events");
        await client.UpdateStatusAsync(
            new DiscordActivity { ActivityType = DiscordActivityType.ListeningTo, Name = "/help" },
            DiscordUserStatus.Online);
    }
}

public sealed class SessionResumedHandler(ILogger<SessionResumedHandler> logger)
{
    public Task Handle(DiscordClient client, SessionResumedEventArgs args)
    {
        logger.LogInformation("Session resumed");
        return Task.CompletedTask;
    }
}

public sealed class ZombiedHandler(ILogger<ZombiedHandler> logger)
{
    public Task Handle(DiscordClient client, ZombiedEventArgs args)
    {
        logger.LogWarning("Zombied connection detected after {Failures} failed heartbeats", args.Failures);
        return Task.CompletedTask;
    }
}
