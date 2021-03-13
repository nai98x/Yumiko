using Discord_Bot;
using DSharpPlus.CommandsNext;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot.DAL
{
    public class CanalAnuncioss
    {
        public long guild_id { get; set; }
        public long channel_id { get; set; }
    }

    public class CanalesAnuncioss
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public void SetCanal(CommandContext ctx, long channelId)
        {

            /*
            using (var context = new YumikoEntities())
            {
                var canal = context.CanalAnunciosSet.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id);
                if(canal == null)
                {
                    canal = new CanalAnuncios()
                    {
                        guild_id = (long)ctx.Guild.Id,
                        channel_id = channelId
                    };
                    context.CanalAnunciosSet.Add(canal);
                }
                else
                {
                    canal.channel_id = channelId;
                }
                context.SaveChanges();
            }
            */
        }

        public async Task<CanalAnuncioss> GetCanal(long guildId)
        {
            var client = funciones.getClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync("CanalesAnuncios/");
            var canales = response.ResultAs<dynamic>();
            int i = 0;
            //return canales.FirstOrDefault(x => x.guild_id == guildId);
            return null;
            /*
            using (var context = new YumikoEntities())
            {
                return context.CanalAnunciosSet.FirstOrDefault(x => x.guild_id == guildId);
            }
            */
        }

        public List<CanalAnuncioss> GetCanales()
        {
            return new List<CanalAnuncioss>();
            /*
            using (var context = new YumikoEntities())
            {
                return context.CanalAnunciosSet.ToList();
            }
            */
        }

        public void DeleteCanal(CommandContext ctx, long channelId)
        {

            /*
            using (var context = new YumikoEntities())
            {
                var canal = context.CanalAnunciosSet.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id);
                if (canal != null)
                {
                    context.CanalAnunciosSet.Remove(canal);
                    context.SaveChanges();
                }
            }
            */
        }
    }
}
