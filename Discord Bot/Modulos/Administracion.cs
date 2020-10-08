using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Administracion : BaseCommandModule
    {
        [Command("reiniciar"), Aliases("restart"), RequireOwner]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar"), RequireOwner]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.RespondAsync("Me voy onii-chan..");
            Environment.Exit(0);
        }
    }
}
