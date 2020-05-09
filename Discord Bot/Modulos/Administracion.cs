using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
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
            //var user = ctx.Member;
            //var canal = user.

            var lista = ctx.Channel.Users;
            Random rnd = new Random();
            var user = lista.ElementAt(rnd.Next(lista.Count()));

            string s= "";
            foreach(var usr in lista)
            {
                s += "  -" + usr.DisplayName + "\n";
            }
            await ctx.Channel.SendMessageAsync(s).ConfigureAwait(false);
            /*
            await ctx.Channel.SendMessageAsync(user.Mention + " se fue MUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(true, "Muteadisimo man");
            await Task.Delay(1000 * 60);
            await ctx.Channel.SendMessageAsync(user.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(true, "Desmutea3");
            */
        }
    }
}
