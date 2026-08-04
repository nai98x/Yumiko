using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.State;

namespace Yumiko.Bot.Events.Handlers;

public sealed class GuildDownloadCompletedHandler(
    DiscordBotService discordBotService,
    AnilistMediaCacheState mediaCache,
    MediaCacheRefresher mediaCacheRefresher,
    ILogger<GuildDownloadCompletedHandler> logger)
{
    public async Task Handle(DiscordClient client, GuildDownloadCompletedEventArgs args)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            discordBotService.SetChannels();
            logger.LogInformation("Guild y canales de log inicializados");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "No se pudieron resolver el guild o los canales de log; se aborta la inicialización");
            discordBotService.SetInitializationFailed();
            return;
        }

        await mediaCacheRefresher.RefreshAsync();

        discordBotService.SetInitialized();
        sw.Stop();

        logger.LogInformation(
            "Bot inicializado en {Segundos:0.00}s | Guilds: {Guilds} | Media en caché: {Media}",
            sw.Elapsed.TotalSeconds,
            client.Guilds.Count,
            mediaCache.Media.Count);
    }
}
