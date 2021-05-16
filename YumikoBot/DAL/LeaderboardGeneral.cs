using Discord_Bot;
using DSharpPlus.CommandsNext;
using FireSharp.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot.DAL
{
    public class Leaderboardo
    {
        public long user_id { get; set; }
        public int partidasJugadas { get; set; }
        public int rondasAcertadas { get; set; }
        public int rondasTotales { get; set; }
    }

    public class LeaderboardoNew
    {
        public int partidasJugadas { get; set; }
        public int rondasAcertadas { get; set; }
        public int rondasTotales { get; set; }
    }

    public class LeaderboardoGeneral
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public async Task<List<Leaderboardo>> GetLeaderboardFirebase(CommandContext ctx, string juego, string dificultad)
        {
            var ret = new List<Leaderboardo>();
            var client = await funciones.GetClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync($"Juegos/{juego}/{dificultad}/{ctx.Guild.Id}/");
            var json = response.Body;
            if(json != "null")
            {
                dynamic items = JsonConvert.DeserializeObject<dynamic>(json);
                foreach (var item in items)
                {
                    ret.Add(new Leaderboardo()
                    {
                        user_id = long.Parse(item.Name),
                        partidasJugadas = item.First.partidasJugadas,
                        rondasAcertadas = item.First.rondasAcertadas,
                        rondasTotales = item.First.rondasTotales,
                    });
                }
            }
            return ret;
        }

        public async Task AddRegistro(CommandContext ctx, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            var clientFirebase = await funciones.GetClienteFirebase();
            var response = await clientFirebase.GetTaskAsync($"Juegos/{juego}/{dificultad}/{ctx.Guild.Id}/{ctx.Member.Id}/");
            if(response.Body.ToLower() == "null")
            {
                await clientFirebase.SetTaskAsync($"Juegos/{juego}/{dificultad}/{ctx.Guild.Id}/{ctx.Member.Id}", new LeaderboardoNew()
                {
                    partidasJugadas = 1,
                    rondasAcertadas = rondasAcertadas,
                    rondasTotales = rondasTotales
                });
            }
            else
            {
                var registro = response.ResultAs<LeaderboardoNew>();
                registro.partidasJugadas++;
                registro.rondasAcertadas += rondasAcertadas;
                registro.rondasTotales += rondasTotales;
                await clientFirebase.UpdateTaskAsync($"Juegos/{juego}/{dificultad}/{ctx.Guild.Id}/{ctx.Member.Id}", registro);
            }
        }

        public async Task<List<StatsJuego>> GetLeaderboard(CommandContext ctx, string dificultad, string juego, bool global)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            var listaFirebase = await GetLeaderboardFirebase(ctx, juego, dificultad);
            /* Comantado hasta unificar las estadisticas de un usuario en distintos servidores
            if (global) 
                list = listaFirebase.Where(x => x.dificultad == dificultad && x.juego == juego);
            else */
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
            var client = await funciones.GetClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync($"Juegos/tag/");
            var json = response.Body;
            dynamic items = JsonConvert.DeserializeObject<dynamic>(json);
            foreach (var item in items)
            {
                string tagName = item.Name;
                FirebaseResponse response2 = await client.GetTaskAsync($"Juegos/tag/{tagName}/");
                var json2 = response2.Body;
                dynamic items2 = JsonConvert.DeserializeObject<dynamic>(json2);
                foreach (var item2 in items2)
                {
                    var guildId = long.Parse(item2.Name);
                    if (ctx.Guild.Id == (ulong)guildId)
                    {
                        ret.Add(tagName);
                    }
                }
            }
            return ret;
        }
    }
}
