using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Application.Migration;

/// <summary>
/// Moves what is in Firestore to PostgreSQL, one section at a time: every section read is imported
/// right away and reported through <c>report</c>, so a run that takes an hour still shows progress.
/// It goes away with Firebase once the migration is done.
/// </summary>
public sealed class FirestoreMigrationService(IFirestoreMigrationSource source, IMigrationRepository repository)
{
    // Sections are one per guild: reporting every one of them would be a message per guild.
    private const int ReportEverySections = 25;

    public async Task<List<MigrationResult>> MigrateAsync(Func<string, Task> report, CancellationToken cancellationToken = default)
    {
        return
        [
            await RunAsync("anilist_users", source.ReadAnilistUsersAsync(cancellationToken), repository.ImportAnilistUsersAsync, report, cancellationToken),
            await RunAsync("higher_or_lower_scores", source.ReadHigherOrLowerAsync(cancellationToken), repository.ImportHigherOrLowerAsync, report, cancellationToken),
            await RunAsync("quiz_stats", source.ReadQuizStatsAsync(cancellationToken), repository.ImportQuizStatsAsync, report, cancellationToken),
        ];
    }

    private static async Task<MigrationResult> RunAsync<T>(
        string table,
        IAsyncEnumerable<MigrationBatch<T>> batches,
        Func<IReadOnlyList<T>, CancellationToken, Task<int>> import,
        Func<string, Task> report,
        CancellationToken cancellationToken)
    {
        await report($"**{table}** — reading Firestore...");

        int read = 0;
        int written = 0;
        int skipped = 0;
        int sections = 0;

        await foreach (MigrationBatch<T> batch in batches.WithCancellation(cancellationToken))
        {
            written += await import(batch.Records, cancellationToken);
            read += batch.Records.Count + batch.Skipped;
            skipped += batch.Skipped;
            sections++;

            if (batch.Skipped > 0)
            {
                await report($"`{table}` section `{batch.Section}`: {batch.Skipped} skipped");
            }
            else if (sections % ReportEverySections == 0)
            {
                await report($"`{table}`: {sections} sections, {written} rows written");
            }
        }

        await report($"**{table}** — done: {read} read, {written} written, {skipped} skipped");

        return new MigrationResult(table, read, written, skipped);
    }
}
