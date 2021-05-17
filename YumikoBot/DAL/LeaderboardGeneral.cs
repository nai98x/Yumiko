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
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public async Task<List<LeaderboardFirebase>> GetLeaderboardFirebase(long guildId, string juego, string dificultad)
        {
            var ret = new List<LeaderboardFirebase>();
            string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            FirestoreDb db = FirestoreDb.Create("yumiko-1590195019393");

            CollectionReference col = db.Collection("Estadisticas").Document($"{guildId}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios");
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

        public async Task AddRegistro(CommandContext ctx, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            FirestoreDb db = FirestoreDb.Create("yumiko-1590195019393");
            DocumentReference doc = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection($"Juegos").Document($"{juego}").Collection($"Dificultad").Document($"{dificultad}").Collection("Usuarios").Document($"{userId}");
            var snap = await doc.GetSnapshotAsync();
            LeaderboardFirebase registro;
            if (snap.Exists)
            {
                registro = snap.ConvertTo<LeaderboardFirebase>();
                registro.partidasJugadas++;
                registro.rondasAcertadas += rondasAcertadas;
                registro.rondasTotales += rondasTotales;
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    {"user_id", registro.user_id},
                    {"partidasJugadas", registro.partidasJugadas},
                    {"rondasAcertadas", registro.rondasAcertadas},
                    {"rondasTotales", registro.rondasTotales},
                };
                await doc.UpdateAsync(data);
            }
            else
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    {"user_id", userId},
                    {"partidasJugadas", 1},
                    {"rondasAcertadas", rondasAcertadas},
                    {"rondasTotales", rondasTotales},
                };
                await doc.SetAsync(data);
            }
        }

        public async Task<List<StatsJuego>> GetLeaderboard(CommandContext ctx, string dificultad, string juego, bool global)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            var listaFirebase = await GetLeaderboardFirebase((long)ctx.Guild.Id, juego, dificultad);
            listaFirebase.ForEach(x =>
            {
                lista.Add(new StatsJuego()
                {
                    UserId = x.user_id,
                    PartidasTotales = x.partidasJugadas,
                    RondasTotales = x.rondasTotales,
                    RondasAcertadas = x.rondasAcertadas,
                    PorcentajeAciertos = (x.rondasAcertadas * 100) / x.rondasTotales
                });
            });
            lista.Sort((x, y) => y.PorcentajeAciertos.CompareTo(x.PorcentajeAciertos));
            return lista.Take(10).ToList();
        }
        
        public async Task<List<string>> GetTags(CommandContext ctx)
        {
            List<string> ret = new List<string>();

            string path = AppDomain.CurrentDomain.BaseDirectory + @"firebase.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            FirestoreDb db = FirestoreDb.Create("yumiko-1590195019393");

            CollectionReference col = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document("tag").Collection("Dificultad");
            var snap = await col.GetSnapshotAsync();

            var col2 = db.Collection("Estadisticas").Document($"{ctx.Guild.Id}").Collection("Juegos").Document("tag");
            var snap2 = await col2.GetSnapshotAsync();

            if (snap.Count > 0)
            {
                foreach (var document in snap.Documents)
                {
                   // ret.Add(document.ConvertTo<LeaderboardFirebase>());
                }
            }

            return ret;
        }
    }
}
