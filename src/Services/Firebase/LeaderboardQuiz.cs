namespace Yumiko.Services.Firebase
{
    using DSharpPlus.SlashCommands;
    using Google.Cloud.Firestore;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Yumiko.Datatypes;
    using Yumiko.Datatypes.Firebase;
    using Yumiko.Utils;

    public static class LeaderboardQuiz
    {
        public static async Task<List<DtLeaderboardQuiz>> GetLeaderboardFirebaseAsync(IConfiguration Configuration, long guildId, string juego, string dificultad, int cantidad)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            var ret = new List<DtLeaderboardQuiz>();

            var col = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios").OrderByDescending("porcentajeAciertos").OrderByDescending("rondasTotales").Limit(cantidad);
            var snap = await col.GetSnapshotAsync();

            if (snap.Count > 0)
            {
                foreach (var document in snap.Documents)
                {
                    ret.Add(document.ConvertTo<DtLeaderboardQuiz>());
                }
            }

            return ret;
        }

        public static async Task AddRegistroAsync(IConfiguration Configuration, long guildId, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            DocumentReference doc = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            DtLeaderboardQuiz registro;
            if (snap.Exists)
            {
                registro = snap.ConvertTo<DtLeaderboardQuiz>();
                registro.partidasJugadas++;
                registro.rondasAcertadas += rondasAcertadas;
                registro.rondasTotales += rondasTotales;
                Dictionary<string, object> data = new()
                {
                    { "user_id", registro.user_id },
                    { "partidasJugadas", registro.partidasJugadas },
                    { "rondasAcertadas", registro.rondasAcertadas },
                    { "rondasTotales", registro.rondasTotales },
                    { "porcentajeAciertos", (registro.rondasAcertadas * 100) / registro.rondasTotales },
                };
                await doc.UpdateAsync(data);
            }
            else
            {
                Dictionary<string, object> data = new()
                {
                    { "user_id", userId },
                    { "partidasJugadas", 1 },
                    { "rondasAcertadas", rondasAcertadas },
                    { "rondasTotales", rondasTotales },
                    { "porcentajeAciertos", (rondasAcertadas * 100) / rondasTotales },
                };
                await doc.SetAsync(data);
            }
        }

        public static async Task<List<GameStats>> GetLeaderboardAsync(InteractionContext ctx, IConfiguration Configuration, string dificultad, string juego)
        {
            List<GameStats> lista = new();
            var listaFirebase = await GetLeaderboardFirebaseAsync(Configuration, (long)ctx.Guild.Id, juego, dificultad, 10);
            listaFirebase.ForEach(x =>
            {
                lista.Add(new GameStats()
                {
                    UserId = x.user_id,
                    PartidasTotales = x.partidasJugadas,
                    RondasTotales = x.rondasTotales,
                    RondasAcertadas = x.rondasAcertadas,
                    PorcentajeAciertos = x.porcentajeAciertos,
                });
            });
            return lista;
        }

        public static async Task EliminarEstadisticasAsync(InteractionContext ctx, IConfiguration Configuration, string juego)
        {
            FirestoreDb db = Common.GetFirestoreClient(Configuration);
            DocumentReference docFacil = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Fácil").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapFacil = await docFacil.GetSnapshotAsync();
            DocumentReference docMedia = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Media").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapMedia = await docMedia.GetSnapshotAsync();
            DocumentReference docDificil = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Dificil").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapDificil = await docDificil.GetSnapshotAsync();
            DocumentReference docExtremo = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Extremo").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapExtremo = await docExtremo.GetSnapshotAsync();

            if (snapFacil.Exists)
            {
                await docFacil.DeleteAsync();
            }

            if (snapMedia.Exists)
            {
                await docMedia.DeleteAsync();
            }

            if (snapDificil.Exists)
            {
                await docDificil.DeleteAsync();
            }

            if (snapExtremo.Exists)
            {
                await docExtremo.DeleteAsync();
            }
        }
    }
}
