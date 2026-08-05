using Google.Cloud.Firestore;
using Yumiko.Infrastructure.Firebase;
using Yumiko.Infrastructure.Firebase.Documents;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class HigherOrLowerLeaderboardRepository(FirebaseService firebase) : IHigherOrLowerLeaderboardRepository
{
    private CollectionReference Users(ulong guildId) =>
        firebase.GetDb().Collection("HigherOrLower").Document($"{guildId}").Collection("Usuarios");

    public async Task<List<HigherOrLowerEntry>> GetLeaderboardAsync(ulong guildId)
    {
        Query query = Users(guildId).OrderByDescending("puntuacion").Limit(20);
        QuerySnapshot snap = await query.GetSnapshotAsync();

        return [.. snap.Documents.Select(d => Map(d.ConvertTo<HigherOrLowerDocument>()))];
    }

    public async Task<HigherOrLowerEntry?> GetStatsUserAsync(ulong guildId, ulong userId)
    {
        DocumentSnapshot snap = await Users(guildId).Document($"{userId}").GetSnapshotAsync();

        return snap.Exists ? Map(snap.ConvertTo<HigherOrLowerDocument>()) : null;
    }

    public async Task<bool> AddResultAsync(ulong guildId, ulong userId, int score)
    {
        DocumentReference doc = Users(guildId).Document($"{userId}");
        DocumentSnapshot snap = await doc.GetSnapshotAsync();

        Dictionary<string, object> data = new()
        {
            { "user_id", (long)userId },
            { "puntuacion", score },
        };

        if (!snap.Exists)
        {
            await doc.CreateAsync(data);
            return true;
        }

        // Only the record is stored: if it is not beaten, the document stays as it is.
        if (score <= snap.ConvertTo<HigherOrLowerDocument>().puntuacion)
        {
            return false;
        }

        await doc.UpdateAsync(data);
        return true;
    }

    public async Task DeleteStatsAsync(ulong guildId, ulong userId)
    {
        DocumentReference doc = Users(guildId).Document($"{userId}");
        DocumentSnapshot snap = await doc.GetSnapshotAsync();

        if (snap.Exists)
        {
            await doc.DeleteAsync();
        }
    }

    private static HigherOrLowerEntry Map(HigherOrLowerDocument doc) => new()
    {
        UserId = (ulong)doc.user_id,
        Score = doc.puntuacion,
    };
}
