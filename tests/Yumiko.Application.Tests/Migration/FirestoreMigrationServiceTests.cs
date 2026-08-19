using Yumiko.Application.Migration;
using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Migration;

public class FirestoreMigrationServiceTests
{
    [Fact]
    public async Task MigrateAsync_ImportsEverySectionAndReportsTheTotals()
    {
        FakeMigrationRepository repository = new();
        FirestoreMigrationService service = new(Source(), repository);

        List<MigrationResult> results = await service.MigrateAsync(_ => Task.CompletedTask);

        Assert.Equal(1, results.Single(r => r.Table == "anilist_users").Written);
        Assert.Equal(3, results.Single(r => r.Table == "higher_or_lower_scores").Written);
        Assert.Equal(1, results.Single(r => r.Table == "quiz_stats").Written);

        // Two guilds arrive as separate sections and both have to end up imported.
        Assert.Equal(3, repository.HigherOrLower.Count);
        Assert.Equal(Gamemode.Genres, repository.QuizStats.Single().Gamemode);
    }

    [Fact]
    public async Task MigrateAsync_ReportsEveryTableStartAndFinish()
    {
        List<string> reported = [];
        FirestoreMigrationService service = new(Source(), new FakeMigrationRepository());

        await service.MigrateAsync(line =>
        {
            reported.Add(line);
            return Task.CompletedTask;
        });

        foreach (string table in (string[])["anilist_users", "higher_or_lower_scores", "quiz_stats"])
        {
            Assert.Contains(reported, line => line.Contains(table, StringComparison.Ordinal) && line.Contains("reading", StringComparison.Ordinal));
            Assert.Contains(reported, line => line.Contains(table, StringComparison.Ordinal) && line.Contains("done", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task MigrateAsync_SectionWithSkippedDocuments_IsReportedAndCountedAsRead()
    {
        List<string> reported = [];

        FakeFirestoreMigrationSource source = new()
        {
            AnilistUsers = [new MigrationBatch<AnilistUserRecord>("AnilistUsers", [new AnilistUserRecord(1, 10)], 4)],
        };

        FirestoreMigrationService service = new(source, new FakeMigrationRepository());

        List<MigrationResult> results = await service.MigrateAsync(line =>
        {
            reported.Add(line);
            return Task.CompletedTask;
        });

        MigrationResult result = results.Single(r => r.Table == "anilist_users");

        Assert.Equal(5, result.Read);
        Assert.Equal(1, result.Written);
        Assert.Equal(4, result.Skipped);
        Assert.Contains(reported, line => line.Contains("4 skipped", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MigrateAsync_EmptyFirestore_ImportsNothing()
    {
        FakeMigrationRepository repository = new();
        FirestoreMigrationService service = new(new FakeFirestoreMigrationSource(), repository);

        List<MigrationResult> results = await service.MigrateAsync(_ => Task.CompletedTask);

        Assert.All(results, result => Assert.Equal(0, result.Written));
        Assert.Empty(repository.AnilistUsers);
    }

    private static FakeFirestoreMigrationSource Source() => new()
    {
        AnilistUsers = [new MigrationBatch<AnilistUserRecord>("AnilistUsers", [new AnilistUserRecord(1, 10)], 0)],
        HigherOrLower =
        [
            new MigrationBatch<HigherOrLowerRecord>("1", [new HigherOrLowerRecord(1, 2, 30), new HigherOrLowerRecord(1, 3, 40)], 0),
            new MigrationBatch<HigherOrLowerRecord>("2", [new HigherOrLowerRecord(2, 2, 50)], 0),
        ],
        QuizStats =
        [
            new MigrationBatch<QuizStatsRecord>("1", [new QuizStatsRecord(1, 2, Gamemode.Genres, "Action", 5, 10, 20, 50)], 0),
        ],
    };
}
