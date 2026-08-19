using Yumiko.Application.Migration;
using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Migration;

public class FirestoreMigrationServiceTests
{
    [Fact]
    public async Task MigrateAsync_ImportsEveryCollectionAndReportsIt()
    {
        FakeMigrationRepository repository = new();
        FirestoreMigrationService service = new(Source(), repository);

        List<MigrationResult> results = await service.MigrateAsync();

        Assert.Equal(1, results.Single(r => r.Table == "anilist_users").Written);
        Assert.Equal(2, results.Single(r => r.Table == "higher_or_lower_scores").Written);
        Assert.Equal(1, results.Single(r => r.Table == "quiz_stats").Written);

        Assert.Equal(2, repository.HigherOrLower.Count);
        Assert.Equal(Gamemode.Genres, repository.QuizStats.Single().Gamemode);
    }

    [Fact]
    public async Task MigrateAsync_EmptyFirestore_ImportsNothing()
    {
        FakeMigrationRepository repository = new();
        FirestoreMigrationService service = new(new FakeFirestoreMigrationSource(), repository);

        List<MigrationResult> results = await service.MigrateAsync();

        Assert.All(results, result => Assert.Equal(0, result.Written));
        Assert.Empty(repository.AnilistUsers);
    }

    [Fact]
    public async Task MigrateAsync_CountsSkippedDocumentsAsRead()
    {
        FakeFirestoreMigrationSource source = new()
        {
            AnilistUsers = [new AnilistUserRecord(1, 10)],
            Skipped = 4,
        };

        FirestoreMigrationService service = new(source, new FakeMigrationRepository());

        MigrationResult result = (await service.MigrateAsync()).Single(r => r.Table == "anilist_users");

        Assert.Equal(5, result.Read);
        Assert.Equal(1, result.Written);
        Assert.Equal(4, result.Skipped);
    }

    private static FakeFirestoreMigrationSource Source() => new()
    {
        AnilistUsers = [new AnilistUserRecord(1, 10)],
        HigherOrLower = [new HigherOrLowerRecord(1, 2, 30), new HigherOrLowerRecord(1, 3, 40)],
        QuizStats = [new QuizStatsRecord(1, 2, Gamemode.Genres, "Action", 5, 10, 20, 50)],
    };
}
