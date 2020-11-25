using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumikoBot.Data_Access_Layer
{
    public class LeaderboardPersonajes
    {
        public void AddRegistro(int userId, int guildId, string dificultad, int porcentajeNuevoAciertos)
        {
            using (var context = new YumikoEntities())
            {
                LeaderboardAn registro = context.LeaderboardAnimes.FirstOrDefault(x => x.user_id == userId && x.guild_id == guildId && x.dificultad == dificultad);
                if(registro == null)
                {
                    LeaderboardAn nuevo = new LeaderboardAn()
                    {
                        user_id = userId,
                        guild_id = guildId,
                        dificultad = dificultad,
                        partidasJugadas = 1,
                        porcentajeAciertos = porcentajeNuevoAciertos
                    };
                    context.LeaderboardAnimes.Add(nuevo);
                }
                else
                {
                    registro.partidasJugadas++;
                    registro.porcentajeAciertos = 1;
                }
                context.SaveChanges();
            }
        }

        public List<LeaderboardAn> GetLeaderboard(int guildId, string dificultad)
        {
            List<LeaderboardAn> lista = null;
            using (var context = new YumikoEntities())
            {

            }
            return lista;
        }
    }
}
