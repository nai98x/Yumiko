using Discord_Bot;
using DSharpPlus.CommandsNext;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot.DAL
{
    public class LeaderboardoGeneral
    {
        private readonly FuncionesAuxiliares funciones = new();

        public async Task<List<LeaderboardFirebase>> GetLeaderboardFirebase(long guildId, string juego, string dificultad)
        {
            var ret = new List<LeaderboardFirebase>();
            FirestoreDb db = funciones.GetFirestoreClient();

            var col = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios").OrderByDescending("porcentajeAciertos").Limit(10);
            var snap = await col.GetSnapshotAsync();

            if (snap.Count > 0)
            {
                foreach(var document in snap.Documents)
                {
                    ret.Add(document.ConvertTo<LeaderboardFirebase>());
                }
            }

            return ret;
        }

        public async Task AddRegistro(long guildId, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            FirestoreDb db = funciones.GetFirestoreClient();
            DocumentReference doc = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            LeaderboardFirebase registro;
            if (snap.Exists)
            {
                registro = snap.ConvertTo<LeaderboardFirebase>();
                registro.partidasJugadas++;
                registro.rondasAcertadas += rondasAcertadas;
                registro.rondasTotales += rondasTotales;
                Dictionary<string, object> data = new()
                {
                    {"user_id", registro.user_id},
                    {"partidasJugadas", registro.partidasJugadas},
                    {"rondasAcertadas", registro.rondasAcertadas},
                    {"rondasTotales", registro.rondasTotales},
                    {"porcentajeAciertos", (registro.rondasAcertadas * 100) / registro.rondasTotales},
                };
                await doc.UpdateAsync(data);
            }
            else
            {
                Dictionary<string, object> data = new()
                {
                    {"user_id", userId},
                    {"partidasJugadas", 1},
                    {"rondasAcertadas", rondasAcertadas},
                    {"rondasTotales", rondasTotales},
                    {"porcentajeAciertos", (rondasAcertadas * 100) / rondasTotales},
                };
                await doc.SetAsync(data);
            }
        }

        public async Task<List<StatsJuego>> GetLeaderboard(Context ctx, string dificultad, string juego)
        {
            List<StatsJuego> lista = new();
            var listaFirebase = await GetLeaderboardFirebase((long)ctx.Guild.Id, juego, dificultad);
            listaFirebase.ForEach(x =>
            {
                lista.Add(new StatsJuego()
                {
                    UserId = x.user_id,
                    PartidasTotales = x.partidasJugadas,
                    RondasTotales = x.rondasTotales,
                    RondasAcertadas = x.rondasAcertadas,
                    PorcentajeAciertos = x.porcentajeAciertos
                });
            });
            return lista;
        }
        
        public async Task<List<string>> GetTags(Context ctx)
        {
            var listaTags = await funciones.GetTags(ctx); // Tags Anilist
            List<string> ret = new();
            FirestoreDb db = funciones.GetFirestoreClient();

            CollectionReference estadisticasRef = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document("tag").Collection("Dificultad");
            IAsyncEnumerable<DocumentReference> subcollections = estadisticasRef.ListDocumentsAsync();
            IAsyncEnumerator<DocumentReference> subcollectionsEnumerator = subcollections.GetAsyncEnumerator(default);
            while (await subcollectionsEnumerator.MoveNextAsync())
            {
                DocumentReference subcollectionRef = subcollectionsEnumerator.Current;
                string tag = subcollectionRef.Id;
                if (listaTags.Where(x => x.Nombre == tag).ToList().Count > 0)
                {
                    ret.Add(tag);
                }
            }
            
            return ret;
        }

        public async Task EliminarEstadisticas(CommandContext ctx, string juego)
        {
            FirestoreDb db = funciones.GetFirestoreClient();

            DocumentReference docFacil = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Fácil").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapFacil = await docFacil.GetSnapshotAsync();
            DocumentReference docMedia = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Media").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapMedia = await docMedia.GetSnapshotAsync();
            DocumentReference docDificil = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Dificil").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapDificil = await docDificil.GetSnapshotAsync();
            DocumentReference docExtremo = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document(juego).Collection("Dificultad").Document("Extremo").Collection("Usuarios").Document($"{ctx.User.Id}");
            var snapExtremo = await docExtremo.GetSnapshotAsync();

            if (snapFacil.Exists)
                await docFacil.DeleteAsync();
            if (snapMedia.Exists)
                await docMedia.DeleteAsync();
            if (snapDificil.Exists)
                await docDificil.DeleteAsync();
            if (snapExtremo.Exists)
                await docExtremo.DeleteAsync();
        }
        public async Task EliminarEstadisticasTag(CommandContext ctx)
        {
            FirestoreDb db = funciones.GetFirestoreClient();

            CollectionReference estadisticasRef = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document("tag").Collection("Dificultad");
            IAsyncEnumerable<DocumentReference> subcollections = estadisticasRef.ListDocumentsAsync();
            IAsyncEnumerator<DocumentReference> subcollectionsEnumerator = subcollections.GetAsyncEnumerator(default);
            while (await subcollectionsEnumerator.MoveNextAsync())
            {
                DocumentReference subcollectionRef = subcollectionsEnumerator.Current;
                DocumentReference doc = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document("tag").Collection("Dificultad").Document(subcollectionRef.Id).Collection("Usuarios").Document($"{ctx.User.Id}");
                var snapExtremo = await doc.GetSnapshotAsync();
                if (snapExtremo.Exists)
                    await doc.DeleteAsync();
            }
        }
    }
}
