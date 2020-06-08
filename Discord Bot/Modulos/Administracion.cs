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
                cantidad = 99;
            }
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(cantidad + 1));
        }

        [Command("mutear")]
        [Aliases("f")]
        [Description("Mutea a un miembro aleatorio del canal")]
        [Cooldown(1,300,CooldownBucketType.Guild)]
        public async Task MutearAleatorio(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Error en la configuración del bot (VoiceNext)");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                DiscordChannel chn = null;
                var vstat = ctx.Member?.VoiceState;
                if (vstat?.Channel == null && chn == null)
                {
                    await ctx.TriggerTypingAsync();
                    await ctx.RespondAsync("No estas en ningun canal, baka");
                    return;
                }
                if (chn == null)
                    chn = vstat.Channel;

                await vnext.ConnectAsync(chn);
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Me he conectado a `{chn.Name}`");
                vnc = vnext.GetConnection(ctx.Guild);
            }

            if (vnc.Channel.Users.Count() == 1)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("Estoy solo yo conectada, baka");
                return;
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

            await ctx.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(user.Mention + " se fue MUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(true, "Muteadisimo man");
            await Task.Delay(1000 * 60);
            await ctx.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(user.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
            await user.SetMuteAsync(false, "Desmutea3");
        }

        [Command("expulsar")]
        [Aliases("kick")]
        [Description("Expulsa a un miembro del servidor")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Expulsar(CommandContext ctx, DiscordMember user)
        {
            if(user != null)
            {
                await user.RemoveAsync("Removido por la diosa Yumiko");
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(user.DisplayName + " se fue BANEADISIMO");
            }
            else
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(ctx.Member.Mention + " menciona bien al que queres banear, pelotudo");
            }
        }

        [Command("reiniciar")]
        [Aliases("restart")]
        [Description("Reinicia a Yumiko")]
        [RequireOwner]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("cerrar")]
        [Description("Apaga a Yumiko")]
        [RequireOwner]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Me voy onii-chan..");
            Environment.Exit(0);
        }

    }
}
