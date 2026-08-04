using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Helpers;

namespace Yumiko.Bot.Events.Handlers;

public sealed class GuildCreatedHandler(DiscordLogService logService, TopggService topggService, ILogger<GuildCreatedHandler> logger)
{
    public async Task Handle(DiscordClient client, GuildCreatedEventArgs args)
    {
        logger.LogInformation("Guild agregado: {Name} | Total: {Count}", args.Guild.Name, client.Guilds.Count);

        await logService.LogGuildAsync(args.Guild, client.Guilds.Count, added: true);
        await topggService.UpdateStatsAsync(client);
    }
}

public sealed class GuildDeletedHandler(DiscordLogService logService, TopggService topggService, ILogger<GuildDeletedHandler> logger)
{
    public async Task Handle(DiscordClient client, GuildDeletedEventArgs args)
    {
        logger.LogInformation("Guild removido: {Name} | Total: {Count}", args.Guild.Name, client.Guilds.Count);

        await logService.LogGuildAsync(args.Guild, client.Guilds.Count, added: false);
        await topggService.UpdateStatsAsync(client);
    }
}
