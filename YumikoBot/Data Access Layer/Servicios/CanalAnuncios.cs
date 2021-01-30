using Discord_Bot;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YumikoBot.Data_Access_Layer
{
    public class CanalesAnuncios
    {
        public void SetCanal(CommandContext ctx, long channelId)
        {
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
        }

        public CanalAnuncios GetCanal(long guildId)
        {
            using (var context = new YumikoEntities())
            {
                return context.CanalAnunciosSet.FirstOrDefault(x => x.guild_id == guildId);
            }
        }

        public List<CanalAnuncios> GetCanales()
        {
            using (var context = new YumikoEntities())
            {
                return context.CanalAnunciosSet.ToList();
            }
        }

        public void DeleteCanal(CommandContext ctx, long channelId)
        {
            using (var context = new YumikoEntities())
            {
                var canal = context.CanalAnunciosSet.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id);
                if (canal != null)
                {
                    context.CanalAnunciosSet.Remove(canal);
                    context.SaveChanges();
                }
            }
        }
    }
}
