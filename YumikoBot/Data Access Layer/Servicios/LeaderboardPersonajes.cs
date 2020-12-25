using Discord_Bot;
using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class LeaderboardPersonajes
    {
        public void AddRegistro(CommandContext ctx, long userId, string dificultad, int rondasAcertadas, int rondasTotales)
        {
            using (var context = new YumikoEntities())
            {
                LeaderboardPj registro = context.LeaderboardPersonajes.FirstOrDefault(x => x.user_id == userId && x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad);
                if(registro == null)
                {
                    LeaderboardPj nuevo = new LeaderboardPj()
                    {
                        user_id = userId,
                        guild_id = (long)ctx.Guild.Id,
                        dificultad = dificultad,
                        partidasJugadas = 1,
                        rondasAcertadas = rondasAcertadas,
                        rondasTotales = rondasTotales
                    };
                    context.LeaderboardPersonajes.Add(nuevo);
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

        public List<StatsJuego> GetLeaderboard(CommandContext ctx, string dificultad)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            using (var context = new YumikoEntities())
            {
                var list = context.LeaderboardPersonajes.ToList().Where(x => x.guild_id == (long)ctx.Guild.Id && x.dificultad == dificultad);
                var listaVerif = ctx.Guild.Members.Values.ToList();
                list.ToList().ForEach(x =>
                {
                    if (listaVerif.Find(u => u.Id == (ulong)x.user_id) != null)
                    {
                        lista.Add(new StatsJuego()
                        {
                            UserId = x.user_id,
                            PartidasTotales = x.partidasJugadas,
                            PorcentajeAciertos = (x.rondasAcertadas * 100) / x.rondasTotales
                        });
                    }
                });
                lista.Sort((x, y) => y.PorcentajeAciertos.CompareTo(x.PorcentajeAciertos));
                return lista.Take(10).ToList();
            }
        }

        public List<StatsJuego> GetStatsUser(CommandContext ctx, long userId)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            using (var context = new YumikoEntities())
            {
                var list = context.LeaderboardPersonajes.ToList().Where(x => x.guild_id == (long)ctx.Guild.Id && x.user_id == userId);
                list.ToList().ForEach(x => {
                    lista.Add(new StatsJuego()
                    {
                        UserId = x.user_id,
                        PartidasTotales = x.partidasJugadas,
                        RondasAcertadas = x.rondasAcertadas,
                        RondasTotales = x.rondasTotales,
                        PorcentajeAciertos = (x.rondasAcertadas * 100) / x.rondasTotales,
                        Dificultad = x.dificultad
                    });
                });
            }
            return lista;
        }
    }
}
