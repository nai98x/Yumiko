using Microsoft.Extensions.DependencyInjection;
using Yumiko.Application.Anilist;
using Yumiko.Application.Extensions;
using Yumiko.Application.Tests.Anilist;
using Yumiko.Application.Tests.Migration;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Application.Tests.Extensions;

public class ApplicationServiceExtensionsTests
{
    [Fact]
    public void AddApplication_RegistersLayerServices()
    {
        ServiceCollection services = new();
        services.AddSingleton<IAnilistClient>(new FakeAnilistClient
        {
            Recommendations = (_, _) => (null, null),
            MediaLists = (_, _, _) => null,
        });
        services.AddSingleton<IFirestoreMigrationSource>(new FakeFirestoreMigrationSource());
        services.AddSingleton<IMigrationRepository>(new FakeMigrationRepository());

        using ServiceProvider provider = services.AddApplication().BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        Assert.NotNull(provider.GetRequiredService<RecommendationService>());
    }
}
