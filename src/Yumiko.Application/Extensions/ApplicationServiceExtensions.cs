using Microsoft.Extensions.DependencyInjection;
using Yumiko.Application.Anilist;
using Yumiko.Application.Migration;

namespace Yumiko.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<RecommendationService>();
        services.AddSingleton<FirestoreMigrationService>();

        return services;
    }
}
