namespace Yumiko.Services.Firebase
{
    using Google.Cloud.Firestore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class LeaderboardHoL
    {
        internal static async Task<List<DtLeaderboardHoL>> GetLeaderboardFirebaseAsync(long guildId)
        {
            FirestoreDb db = Common.GetFirestoreClient();
            var ret = new List<DtLeaderboardHoL>();
            var col = db.Collection("HigherOrLower").Document($"{guildId}").Collection("Usuarios").OrderByDescending("puntuacion").Limit(20);
            var snap = await col.GetSnapshotAsync();

            if (snap.Count > 0)
            {
                foreach (var document in snap.Documents)
                {
                    ret.Add(document.ConvertTo<DtLeaderboardHoL>());
                }
            }

            return ret;
        }

        public static async Task<bool> AddRegistroAsync(ulong userId, ulong guildId, int puntuacion)
        {
            FirestoreDb db = Common.GetFirestoreClient();
            bool actualizar = false;
            DocumentReference doc = db.Collection("HigherOrLower").Document($"{guildId}").Collection("Usuarios").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            DtLeaderboardHoL registro;
            if (snap.Exists)
            {
                registro = snap.ConvertTo<DtLeaderboardHoL>();

                // Actualizo si supero el record
                if (puntuacion > registro.puntuacion)
                {
                    Dictionary<string, object> data = new()
                    {
                        { "user_id", userId },
                        { "puntuacion", puntuacion },
                    };
                    await doc.UpdateAsync(data);
                    actualizar = true;
                }
            }
            else
            {
                actualizar = true;
                Dictionary<string, object> data = new()
                {
                    { "user_id", userId },
                    { "puntuacion", puntuacion },
                };
                await doc.CreateAsync(data);
            }

            return actualizar;
        }

        public static async Task<List<DtLeaderboardHoL>> GetLeaderboardAsync(InteractionContext ctx)
        {
            return await GetLeaderboardFirebaseAsync((long)ctx.Guild.Id);
        }

        public static async Task EliminarEstadisticasAsync(InteractionContext ctx)
        {
            FirestoreDb db = Common.GetFirestoreClient();
            DocumentReference doc = db.Collection("HigherOrLower").Document($"{ctx.Guild.Id}").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snap = await doc.GetSnapshotAsync();

            if (snap.Exists)
            {
                await doc.DeleteAsync();
            }
        }
    }
}
