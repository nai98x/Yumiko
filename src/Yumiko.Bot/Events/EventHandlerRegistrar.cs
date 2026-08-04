using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Yumiko.Bot.Events.Handlers;

namespace Yumiko.Bot.Events;

public static class EventHandlerRegistrar
{
    public static IServiceCollection AddDiscordEventHandlers(this IServiceCollection services)
    {
        services.AddSingleton<SessionCreatedHandler>();
        services.AddSingleton<SessionResumedHandler>();
        services.AddSingleton<ZombiedHandler>();
        services.AddSingleton<GuildDownloadCompletedHandler>();
        services.AddSingleton<GuildCreatedHandler>();
        services.AddSingleton<GuildDeletedHandler>();
        services.AddSingleton<ComponentInteractionHandler>();
        return services;
    }

    // Cada handler se resuelve recién cuando se dispara el evento, vía c.ServiceProvider. Esto rompe
    // el ciclo DiscordClient -> Handler -> DiscordBotService -> DiscordClient del arranque y permite
    // que los handlers inyecten sus dependencias por constructor.
    public static EventHandlingBuilder BindEventHandlers(this EventHandlingBuilder builder)
    {
        return builder
            .HandleSessionCreated((c, e) => c.ServiceProvider.GetRequiredService<SessionCreatedHandler>().Handle(c, e))
            .HandleSessionResumed((c, e) => c.ServiceProvider.GetRequiredService<SessionResumedHandler>().Handle(c, e))
            .HandleZombied((c, e) => c.ServiceProvider.GetRequiredService<ZombiedHandler>().Handle(c, e))
            .HandleGuildDownloadCompleted((c, e) => c.ServiceProvider.GetRequiredService<GuildDownloadCompletedHandler>().Handle(c, e))
            .HandleGuildCreated((c, e) => c.ServiceProvider.GetRequiredService<GuildCreatedHandler>().Handle(c, e))
            .HandleGuildDeleted((c, e) => c.ServiceProvider.GetRequiredService<GuildDeletedHandler>().Handle(c, e))
            .HandleComponentInteractionCreated((c, e) => c.ServiceProvider.GetRequiredService<ComponentInteractionHandler>().Handle(c, e));
    }
}
