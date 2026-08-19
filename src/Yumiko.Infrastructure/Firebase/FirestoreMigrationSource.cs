using System.Globalization;
using System.Runtime.CompilerServices;
using Google.Cloud.Firestore;
using Yumiko.Application.Games;
using Yumiko.Infrastructure.Firebase.Documents;
using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.Firebase;

internal sealed class FirestoreMigrationSource(FirebaseService firebase) : IFirestoreMigrationSource
{
    public async IAsyncEnumerable<MigrationBatch<AnilistUserRecord>> ReadAnilistUsersAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<AnilistUserRecord> records = [];
        int skipped = 0;

        QuerySnapshot snapshot = await firebase.GetDb().Collection("AnilistUsers").GetSnapshotAsync(cancellationToken);

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (!TryParseId(document.Id, out ulong userId))
            {
                skipped++;
                continue;
            }

            records.Add(new AnilistUserRecord(userId, document.ConvertTo<AnilistUserDocument>().AnilistId));
        }

        // A single flat collection: one query, one section.
        yield return new MigrationBatch<AnilistUserRecord>("AnilistUsers", records, skipped);
    }

    public async IAsyncEnumerable<MigrationBatch<HigherOrLowerRecord>> ReadHigherOrLowerAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // The guild documents hold no fields, only the "Usuarios" subcollection: they have to be
        // listed instead of queried.
        await foreach (DocumentReference guild in firebase.GetDb().Collection("HigherOrLower").ListDocumentsAsync().WithCancellation(cancellationToken))
        {
            List<HigherOrLowerRecord> records = [];
            int skipped = 0;

            QuerySnapshot users = await guild.Collection("Usuarios").GetSnapshotAsync(cancellationToken);

            foreach (DocumentSnapshot user in users.Documents)
            {
                if (!TryParseId(guild.Id, out ulong guildId) || !TryParseId(user.Id, out ulong userId))
                {
                    skipped++;
                    continue;
                }

                records.Add(new HigherOrLowerRecord(guildId, userId, user.ConvertTo<HigherOrLowerDocument>().puntuacion));
            }

            if (records.Count > 0 || skipped > 0)
            {
                yield return new MigrationBatch<HigherOrLowerRecord>(guild.Id, records, skipped);
            }
        }
    }

    public async IAsyncEnumerable<MigrationBatch<QuizStatsRecord>> ReadQuizStatsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        CollectionReference root = firebase.GetDb().Collection("Estadisticas");

        await foreach (DocumentReference guild in root.ListDocumentsAsync().WithCancellation(cancellationToken))
        {
            List<QuizStatsRecord> records = [];
            int skipped = 0;

            await foreach (DocumentReference game in guild.Collection("Juegos").ListDocumentsAsync().WithCancellation(cancellationToken))
            {
                Gamemode? gamemode = GameNaming.GamemodeFromSpanish(game.Id);

                await foreach (DocumentReference difficulty in game.Collection("Dificultad").ListDocumentsAsync().WithCancellation(cancellationToken))
                {
                    // In genres mode the "Dificultad" document is the genre name, and it travels as it is.
                    string? difficultyName = gamemode == Gamemode.Genres
                        ? difficulty.Id
                        : GameNaming.DifficultyFromSpanish(difficulty.Id) is { } parsed
                            ? System.Enum.GetName(parsed)
                            : null;

                    QuerySnapshot users = await difficulty.Collection("Usuarios").GetSnapshotAsync(cancellationToken);

                    foreach (DocumentSnapshot user in users.Documents)
                    {
                        if (gamemode is null
                            || difficultyName is null
                            || !TryParseId(guild.Id, out ulong guildId)
                            || !TryParseId(user.Id, out ulong userId))
                        {
                            skipped++;
                            continue;
                        }

                        QuizStatsDocument stats = user.ConvertTo<QuizStatsDocument>();

                        records.Add(new QuizStatsRecord(
                            guildId,
                            userId,
                            gamemode.Value,
                            difficultyName,
                            stats.partidasJugadas,
                            stats.rondasAcertadas,
                            stats.rondasTotales,
                            stats.porcentajeAciertos));
                    }
                }
            }

            if (records.Count > 0 || skipped > 0)
            {
                yield return new MigrationBatch<QuizStatsRecord>(guild.Id, records, skipped);
            }
        }
    }

    private static bool TryParseId(string id, out ulong value) =>
        ulong.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}
