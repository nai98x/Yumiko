namespace Yumiko.Services.Firebase
{
    using Google.Cloud.Firestore;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class UsuariosAnilist
    {
        public static async Task<DtAnilistUser?> GetPerfilAsync(IConfiguration Configuration, ulong userId)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            DocumentReference doc = db.Collection("AnilistUsers").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            if (snap.Exists)
            {
                return snap.ConvertTo<DtAnilistUser>();
            }
            else
            {
                return null;
            }
        }

        public static async Task SetAnilistAsync(IConfiguration Configuration, int anilistId, ulong userId)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            DocumentReference doc = db.Collection("AnilistUsers").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            DtAnilistUser registro;
            if (snap.Exists)
            {
                registro = snap.ConvertTo<DtAnilistUser>();

                registro.AnilistId = anilistId;
                registro.UserId = (long)userId;
                Dictionary<string, object> data = new()
                {
                    { "AnilistId", registro.AnilistId },
                    { "UserId", registro.UserId },
                };
                await doc.UpdateAsync(data);
            }
            else
            {
                Dictionary<string, object> data = new()
                {
                    { "AnilistId", anilistId },
                    { "UserId", userId },
                };
                await doc.SetAsync(data);
            }
        }

        public static async Task<bool> DeleteAnilistAsync(IConfiguration Configuration, ulong userId)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            DocumentReference doc = db.Collection("AnilistUsers").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            if (snap.Exists)
            {
                await doc.DeleteAsync();
                return true;
            }

            return false;
        }
    }
}