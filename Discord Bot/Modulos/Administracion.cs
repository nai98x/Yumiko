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
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        
        [Command("reiniciar")]
        [Aliases("restart")]
        [Description("Reinicia a Yumiko")]
        [Hidden]
        [RequireOwner]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar")]
        [Description("Apaga a Yumiko")]
        [Hidden]
        [RequireOwner]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Me voy onii-chan..");
            Environment.Exit(0);
        }
    }
}
