using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Application.Migration;

/// <summary>
/// Moves everything that is in Firestore to PostgreSQL in one go. It is only used by the owner
/// command and it goes away with Firebase once the migration is done.
/// </summary>
public sealed class FirestoreMigrationService(IFirestoreMigrationSource source, IMigrationRepository repository)
{
    public async Task<List<MigrationResult>> MigrateAsync(CancellationToken cancellationToken = default)
    {
        (List<AnilistUserRecord> anilistUsers, int anilistSkipped) = await source.ReadAnilistUsersAsync(cancellationToken);
        int anilistWritten = await repository.ImportAnilistUsersAsync(anilistUsers, cancellationToken);

        (List<HigherOrLowerRecord> scores, int scoresSkipped) = await source.ReadHigherOrLowerAsync(cancellationToken);
        int scoresWritten = await repository.ImportHigherOrLowerAsync(scores, cancellationToken);

        (List<QuizStatsRecord> stats, int statsSkipped) = await source.ReadQuizStatsAsync(cancellationToken);
        int statsWritten = await repository.ImportQuizStatsAsync(stats, cancellationToken);

        return
        [
            new MigrationResult("anilist_users", anilistUsers.Count + anilistSkipped, anilistWritten, anilistSkipped),
            new MigrationResult("higher_or_lower_scores", scores.Count + scoresSkipped, scoresWritten, scoresSkipped),
            new MigrationResult("quiz_stats", stats.Count + statsSkipped, statsWritten, statsSkipped),
        ];
    }
}
