using Microsoft.Extensions.DependencyInjection;
using Yumiko.Application.Anilist;

namespace Yumiko.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<RecommendationService>();

        return services;
    }
}
