using Discord_Bot;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class LeaderboardPersonajes
    {
        public void AddRegistro(long userId, long guildId, string dificultad, int rondasAcertadas, int rondasTotales)
        {
            using (var context = new YumikoEntities())
            {
                LeaderboardPj registro = context.LeaderboardPersonajes.FirstOrDefault(x => x.user_id == userId && x.guild_id == guildId && x.dificultad == dificultad);
                if(registro == null)
                {
                    LeaderboardPj nuevo = new LeaderboardPj()
                    {
                        user_id = userId,
                        guild_id = guildId,
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

        public List<StatsJuego> GetLeaderboard(long guildId, string dificultad)
        {
            List<StatsJuego> lista = new List<StatsJuego>();
            using (var context = new YumikoEntities())
            {
                var list = context.LeaderboardPersonajes.ToList().Where(x => x.guild_id == guildId && x.dificultad == dificultad);
                list.ToList().ForEach(x =>
                {
                    lista.Add(new StatsJuego() { 
                        UserId = x.user_id,
                        PartidasTotales = x.partidasJugadas,
                        PorcentajeAciertos = (x.rondasAcertadas * 100) / x.rondasTotales
                    });
                });
                lista.Sort((x, y) => y.PorcentajeAciertos.CompareTo(x.PorcentajeAciertos));
                return lista;
            }
        }
    }
}
