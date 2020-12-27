using Discord_Bot;
using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class LeaderboardGeneral
    {
        public void AddRegistro(CommandContext ctx, long userId, string dificultad, int rondasAcertadas, int rondasTotales, string juego)
        {
            using (var context = new YumikoEntities())
            {
                Leaderboard registro = context.LeaderboardSet.FirstOrDefault(x => x.user_id == userId && x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad && x.juego == juego);
                if(registro == null)
                {
                    Leaderboard nuevo = new Leaderboard()
                    {
                        user_id = userId,
                        guild_id = (long)ctx.Guild.Id,
                        dificultad = dificultad,
                        partidasJugadas = 1,
                        rondasAcertadas = rondasAcertadas,
                        rondasTotales = rondasTotales,
                        juego = juego
                    };
                    context.LeaderboardSet.Add(nuevo);
                }
                else
                {
                    registro.partidasJugadas++;
                    registro.rondasAcertadas += rondasAcertadas;
                    registro.rondasTotales += rondasTotales;
                }
                context.SaveChanges();
            }
        }

        public List<StatsJuego> GetLeaderboard(CommandContext ctx, string dificultad, string juego)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            using (var context = new YumikoEntities())
            {
                var list = context.LeaderboardSet.ToList().Where(x => x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad && x.juego == juego);
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
        }

        public StatsJuego GetStatsUser(CommandContext ctx, long userId, string juego, string dificultad)
        {
            using (var context = new YumikoEntities())
            {
                Leaderboard registro = context.LeaderboardSet.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id && x.user_id == userId && x.juego == juego && x.dificultad == dificultad);
                if(registro != null)
                {
                    return new StatsJuego()
                    {
                        UserId = registro.user_id,
                        PartidasTotales = registro.partidasJugadas,
                        RondasAcertadas = registro.rondasAcertadas,
                        RondasTotales = registro.rondasTotales,
                        PorcentajeAciertos = (registro.rondasAcertadas * 100) / registro.rondasTotales,
                        Dificultad = registro.dificultad
                    };
                }
                return null;
            }
        }
    }
}
