using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    //[Group("mememan", CanInvokeWithoutSubcommand = true), Hidden)]
    //[Aliases("adm", "admin")]
    //[Description("Comandos administrativos")]
    public class Administracion : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("clear")]
        [Aliases("c", "borrar")]
        [Description("Borra cierta cantidad de mensajes, requiere permisos")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Clear(CommandContext ctx, [Description("Cantidad de mensajes a borrar")]int cantidad)
        {
            if (cantidad > 99)
            {
                //await ctx.Channel.SendMessageAsync("Advertencia: Se eliminaran unicamente 100 mensajes").ConfigureAwait(false);
                cantidad = 99;
            }
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(cantidad + 1));
        }

        [Command("mutear")]
        [Aliases("f")]
        [Description("Mutea a un miembro aleatorio del canal")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MutearAleatorio(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Error en la configuración del bot (VoiceNext)");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("No estoy conectada, baka");
                return;
            }

            if (vnc.Channel.Users.Count() == 1)
            {
                await ctx.RespondAsync("Estoy solo yo conectada, baka");
            }

            var lista = vnc.Channel.Users;
            Random rnd;

            DiscordMember user;
            while (true)
            {
                rnd = new Random();
                user = lista.ElementAt(rnd.Next(lista.Count()));
                if (user.Id != ctx.Client.CurrentUser.Id)
                {
                    break;
                }
            }

            await ctx.Channel.SendMessageAsync(user.Mention + " se fue MUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(true, "Muteadisimo man");
            await Task.Delay(1000 * 60);
            await ctx.Channel.SendMessageAsync(user.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(false, "Desmutea3");
        }


    }
}
