using Discord_Bot;
using DSharpPlus.CommandsNext;
using FireSharp.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot.DAL
{
    public class Leaderboardo
    {
        public int Id { get; set; }
        public long user_id { get; set; }
        public long guild_id { get; set; }
        public string juego { get; set; }
        public string dificultad { get; set; }
        public int partidasJugadas { get; set; }
        public int rondasAcertadas { get; set; }
        public int rondasTotales { get; set; }
    }

    public class LeaderboardoGeneral
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public async Task<List<Leaderboardo>> GetLeaderboardFirebase()
        {
            var client = await funciones.GetClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync("Leaderboard/");
            var listaFirebase = response.ResultAs<List<Leaderboardo>>();
            return listaFirebase.Where(x => x != null).ToList();
        }

        public async Task<int> GetLastId()
        {
            var lista = await GetLeaderboardFirebase();
            return lista.Last().Id;
        }

        public async Task AddRegistro(CommandContext ctx, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            var clientFirebase = await funciones.GetClienteFirebase();
            var listaFirebase = await GetLeaderboardFirebase();
            var registro = listaFirebase.FirstOrDefault(x => x.user_id == userId && x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad && x.juego == juego);
            if (registro == null)
            {
                int nuevoId = await GetLastId() + 1;
                await clientFirebase.SetTaskAsync("Leaderboard/" + nuevoId, new Leaderboardo()
                {
                    Id = nuevoId,
                    user_id = userId,
                    guild_id = (long)ctx.Guild.Id,
                    dificultad = dificultad,
                    partidasJugadas = 1,
                    rondasAcertadas = rondasAcertadas,
                    rondasTotales = rondasTotales,
                    juego = juego
                });
            }
            else
            {
                registro.partidasJugadas++;
                registro.rondasAcertadas += rondasAcertadas;
                registro.rondasTotales += rondasTotales;
                await clientFirebase.UpdateTaskAsync("Leaderboard/" + registro.Id, registro);
            }
        }

        public async Task<List<StatsJuego>> GetLeaderboard(CommandContext ctx, string dificultad, string juego)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            var listaFirebase = await GetLeaderboardFirebase();
            var list = listaFirebase.Where(x => x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad && x.juego == juego);
            var listaVerif = ctx.Guild.Members.Values.ToList();
            list.ToList().ForEach(x =>
            {
                if (listaVerif.Find(u => u.Id == (ulong)x.user_id) != null)
                {
                    lista.Add(new StatsJuego()
                    {
                        UserId = x.user_id,
                        PartidasTotales = x.partidasJugadas,
                        RondasTotales = x.rondasTotales,
                        RondasAcertadas = x.rondasAcertadas,
                        PorcentajeAciertos = (x.rondasAcertadas * 100) / x.rondasTotales
                    });
                }
            });
            lista.Sort((x, y) => y.PorcentajeAciertos.CompareTo(x.PorcentajeAciertos));
            return lista.Take(10).ToList();
        }
        
        public async Task<List<string>> GetTags()
        {
            var listaFirebase = await GetLeaderboardFirebase();
            var list = listaFirebase.Where(x => x.juego == "tag").ToList();
            var distinctTags = list
              .GroupBy(p => p.dificultad)
              .Select(g => g.First().dificultad)
              .ToList();
            distinctTags.Sort((x, y) => x.CompareTo(y));
            return distinctTags;
        }
    }
}
