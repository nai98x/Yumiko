using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Yumiko.Infrastructure.Animals;
using Yumiko.Infrastructure.Anilist;
using Yumiko.Infrastructure.AnimeThemes;
using Yumiko.Infrastructure.Firebase;
using Yumiko.Infrastructure.OpenWeather;
using Yumiko.Infrastructure.Repositories;
using Yumiko.Infrastructure.Topgg;
using Yumiko.Infrastructure.TraceMoe;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string firebaseCredentialsDir, ExternalApiTokens tokens)
    {
        services.AddSingleton(new FirebaseService(firebaseCredentialsDir));
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

        services.AddHttpClient<IAnimeThemesClient, AnimeThemesClient>(client =>
            client.BaseAddress = new Uri("https://api.animethemes.moe/"));

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
