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
        public int Id { get; set; }
        public long guild_id { get; set; }
        public long channel_id { get; set; }
    }

    public class CanalesAnuncioss
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        public async Task<List<CanalAnuncioss>> GetListaCA()
        {
            var client = await funciones.getClienteFirebase();
            FirebaseResponse response = await client.GetTaskAsync("CanalesAnuncios/");
            var listaFirebase = response.ResultAs<List<CanalAnuncioss>>();
            return listaFirebase.Where(x => x != null).ToList();
        }

        public async Task<int> GetLastId()
        {
            var lista = await GetListaCA();
            return lista.Last().Id;
        }

        public async Task SetCanal(CommandContext ctx, long channelId)
        {
            var client = await funciones.getClienteFirebase();
            var listaFirebase = await GetListaCA();
            var canal = listaFirebase.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id);
            if (canal == null)
            {
                int nuevoId = await GetLastId() + 1;
                await client.SetTaskAsync("CanalesAnuncios/" + nuevoId, new CanalAnuncioss
                {
                    Id = nuevoId,
                    channel_id = channelId,
                    guild_id = (long)ctx.Guild.Id
                });
            }
            else
            {
                await client.UpdateTaskAsync("CanalesAnuncios/" + canal.Id, new CanalAnuncioss
                {
                    Id = canal.Id,
                    channel_id = channelId,
                    guild_id = (long)ctx.Guild.Id
                });
            }
        }

        public async Task<CanalAnuncioss> GetCanal(long guildId)
        {
            var canales = await GetListaCA();
            return canales.FirstOrDefault(x => x.guild_id == guildId);
        }

        public async Task<List<CanalAnuncioss>> GetCanales()
        {
            return await GetListaCA();
        }

        public async Task DeleteCanal(CommandContext ctx)
        {
            var client = await funciones.getClienteFirebase();
            var listaFirebase = await GetListaCA();
            var canal = listaFirebase.FirstOrDefault(x => x.guild_id == (long)ctx.Guild.Id);
            if (canal != null)
            {
                await client.DeleteTaskAsync("CanalesAnuncios/" + canal.Id);
            }
        }
    }
}
