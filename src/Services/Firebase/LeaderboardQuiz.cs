namespace Yumiko.Services.Firebase
{
    using Google.Cloud.Firestore;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class LeaderboardQuiz
    {
        public static async Task<List<DtLeaderboardQuiz>> GetLeaderboardFirebaseAsync(long guildId, string juego, string dificultad, int cantidad)
        {
            FirestoreDb db = Common.GetFirestoreClient();
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

        public static async Task AddRegistroAsync(long guildId, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            FirestoreDb db = Common.GetFirestoreClient();
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

        public static async Task<List<GameStats>> GetLeaderboardAsync(InteractionContext ctx, string dificultad, string juego)
        {
            List<GameStats> lista = new();
            var listaFirebase = await GetLeaderboardFirebaseAsync((long)ctx.Guild.Id, juego, dificultad, 10);
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

        public static async Task EliminarEstadisticasAsync(InteractionContext ctx, string juego)
        {
            FirestoreDb db = Common.GetFirestoreClient();
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

        public static async Task<List<GameStatsUser>> GetStatsUserAsync(ulong guildId, ulong userId)
        {
            List<GameStatsUser> ret = new();
            FirestoreDb db = Common.GetFirestoreClient();

            foreach (Gamemode gm in Enum.GetValues(typeof(Gamemode)))
            {
                var gamemode = gm.ToSpanish();
                var gameStats = new GameStatsUser
                {
                    Gamemode = gm
                };
                foreach (Difficulty df in Enum.GetValues(typeof(Difficulty)))
                {
                    var difficulty = df.ToSpanish();
                    DocumentReference doc = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{gamemode}").Collection($"Dificultad").Document($"{difficulty}").Collection("Usuarios").Document($"{userId}");
                    var snap = await doc.GetSnapshotAsync();
                    if (snap.Exists)
                    {
                        DtLeaderboardQuiz registro = snap.ConvertTo<DtLeaderboardQuiz>();
                        gameStats.Stats.Add(new GameStats()
                        {
                            UserId = registro.user_id,
                            PartidasTotales = registro.partidasJugadas,
                            RondasTotales = registro.rondasTotales,
                            RondasAcertadas = registro.rondasAcertadas,
                            PorcentajeAciertos = registro.porcentajeAciertos,
                            Dificultad = df.GetName()
                        });
                    }
                }
                ret.Add(gameStats);
            }

            return ret;
        }

        public static async Task<List<GameStats>> GetGenreStatsUserAsync(ulong guildId, ulong userId)
        {
            List<GameStats> ret = new();
            FirestoreDb db = Common.GetFirestoreClient();
            CollectionReference colGenres = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"genero").Collection($"Dificultad");
            IAsyncEnumerable<DocumentReference> subcollectionsGenres = colGenres.ListDocumentsAsync();
            IAsyncEnumerator<DocumentReference> subcollectionsEnumerator = subcollectionsGenres.GetAsyncEnumerator(default);
            while (await subcollectionsEnumerator.MoveNextAsync())
            {
                DocumentReference subcollectionRef = subcollectionsEnumerator.Current;
                DocumentReference doc = db.Collection("Estadisticas").Document($"{guildId}").Collection("Juegos").Document("genero").Collection("Dificultad").Document(subcollectionRef.Id).Collection("Usuarios").Document($"{userId}");
                var snap = await doc.GetSnapshotAsync();
                if (snap.Exists)
                {
                    DtLeaderboardQuiz registro = snap.ConvertTo<DtLeaderboardQuiz>();
                    ret.Add(new GameStats()
                    {
                        UserId = registro.user_id,
                        PartidasTotales = registro.partidasJugadas,
                        RondasTotales = registro.rondasTotales,
                        RondasAcertadas = registro.rondasAcertadas,
                        PorcentajeAciertos = registro.porcentajeAciertos,
                        Dificultad = subcollectionRef.Id
                    });
                }
            }

            return ret;
        }
    }
}
