using Google.Cloud.Firestore;
using Yumiko.Infrastructure.Firebase;
using Yumiko.Infrastructure.Firebase.Documents;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class AnilistUsersRepository(FirebaseService firebase) : IAnilistUsersRepository
{
    private DocumentReference DocumentFor(ulong userId) =>
        firebase.GetDb().Collection("AnilistUsers").Document($"{userId}");

    public async Task<AnilistUserLink?> GetLinkAsync(ulong userId)
    {
        DocumentSnapshot snap = await DocumentFor(userId).GetSnapshotAsync();

        if (!snap.Exists)
        {
            return null;
        }

        AnilistUserDocument doc = snap.ConvertTo<AnilistUserDocument>();
        return new AnilistUserLink
        {
            AnilistId = doc.AnilistId,
            UserId = (ulong)doc.UserId,
        };
    }

    public async Task SetAnilistAsync(int anilistId, ulong userId)
    {
        DocumentReference doc = DocumentFor(userId);
        DocumentSnapshot snap = await doc.GetSnapshotAsync();

        Dictionary<string, object> data = new()
        {
            { "AnilistId", anilistId },
            { "UserId", (long)userId },
        };

        if (snap.Exists)
        {
            await doc.UpdateAsync(data);
        }
        else
        {
            await doc.SetAsync(data);
        }
    }

    public async Task<bool> DeleteAnilistAsync(ulong userId)
    {
        DocumentReference doc = DocumentFor(userId);
        DocumentSnapshot snap = await doc.GetSnapshotAsync();

        if (!snap.Exists)
        {
            return false;
        }

        await doc.DeleteAsync();
        return true;
    }
}
