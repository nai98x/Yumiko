using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yumiko.Infrastructure.Http;
using Yumiko.Infrastructure.Animals;
using Yumiko.Infrastructure.Anilist;
using Yumiko.Infrastructure.AnimeThemes;
using Yumiko.Infrastructure.Database;
using Yumiko.Infrastructure.OpenWeather;
using Yumiko.Infrastructure.Repositories;
using Yumiko.Infrastructure.Topgg;
using Yumiko.Infrastructure.TraceMoe;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    // animethemes.moe answers 403 to requests without a User-Agent, and HttpClient does not send one
    // by default. It applies to both its REST and its GraphQL host.
    private const string UserAgent = "Yumiko/1.0 (+https://github.com/nai98x/Yumiko)";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString, ExternalApiTokens tokens)
    {
        services.AddSingleton(new DbConnectionFactory(dbConnectionString));
        services.AddSingleton<IQuizLeaderboardRepository, QuizLeaderboardRepository>();
        services.AddSingleton<IHigherOrLowerLeaderboardRepository, HigherOrLowerLeaderboardRepository>();
        services.AddSingleton<IAnilistUsersRepository, AnilistUsersRepository>();
        services.AddSingleton<AnilistGraphQLExecutor>();
        services.AddSingleton<IAnilistClient, AnilistClient>();

        services.AddHttpClient<IWeatherClient, OpenWeatherClient>(client =>
            client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/"))
            .AddTypedClient<IWeatherClient>(http => new OpenWeatherClient(http, tokens.OpenWeatherMap));

        services.AddHttpClient<IAnimalImageClient, AnimalImageClient>()
            .AddTypedClient<IAnimalImageClient>(http => new AnimalImageClient(http, tokens.TheCatApi, tokens.TheDogApi));

        services.AddHttpClient<ITraceMoeClient, TraceMoeClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.trace.moe/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // animethemes.moe documents a limit of 90 requests per minute; the window is the fallback
        // for when the response does not report X-RateLimit-Reset.
        RateLimitState animeThemesRateLimit = new(TimeSpan.FromMinutes(1));

        services.AddHttpClient<IAnimeThemesClient, AnimeThemesClient>(client =>
        {
            client.BaseAddress = new Uri("https://graphql.animethemes.moe/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler(provider => new RateLimitHandler(
            animeThemesRateLimit,
            provider.GetRequiredService<ILoggerFactory>().CreateLogger<RateLimitHandler>(),
            "animethemes.moe"));

        services.AddHttpClient<ITopggClient, TopggClient>(client =>
        {
            client.BaseAddress = new Uri("https://top.gg/api/");
            if (!string.IsNullOrEmpty(tokens.Topgg))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", tokens.Topgg);
            }
        });

        return services;
    }
}
