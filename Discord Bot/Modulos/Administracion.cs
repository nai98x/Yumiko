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

    }
}
