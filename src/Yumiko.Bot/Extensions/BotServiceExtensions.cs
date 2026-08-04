using Microsoft.Extensions.DependencyInjection;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Games;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.Scheduling.Tasks;
using Yumiko.Bot.Services.State;

namespace Yumiko.Bot.Extensions;

public static class BotServiceExtensions
{
    public static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<ILocalizer, ResxLocalizer>();
        services.AddSingleton<ResxInteractionLocalizer>();

        services.AddSingleton<TriviaState>();
        services.AddSingleton<PollState>();
        services.AddSingleton<CommandUsageState>();
        services.AddSingleton<AnilistMediaCacheState>();

        services.AddSingleton<CountriesCatalog>();
        services.AddSingleton<DiscordInteractivity>();
        services.AddSingleton<AnithemeSelector>();
        services.AddSingleton<AnilistResponses>();
        services.AddSingleton<GenreSelector>();

        services.AddSingleton<GamePool>();
        services.AddSingleton<TriviaGameRunner>();
        services.AddSingleton<HangmanGameRunner>();
        services.AddSingleton<HigherOrLowerGameRunner>();
        services.AddSingleton<TicTacToeGameRunner>();
        services.AddSingleton<DiscordLogService>();
        services.AddSingleton<TopggService>();

        services.AddSingleton<MediaCacheRefresher>();

        services.AddSingleton<DiscordBotService>();
        services.AddHostedService(sp => sp.GetRequiredService<DiscordBotService>());
        services.AddHostedService<DailyScheduledService>();

        return services;
    }
}
