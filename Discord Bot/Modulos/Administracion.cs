using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Administracion : BaseCommandModule
    {
        [Command("reiniciar"), Aliases("restart"), RequireOwner]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
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
