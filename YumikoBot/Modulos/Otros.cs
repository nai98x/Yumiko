using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("reiniciar"), Aliases("restart"), RequireOwner, Description("Se reinicia Yumiko.")]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar"), RequireOwner, Description("Se apaga Yumiko.")]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.RespondAsync("Me voy onii-chan..");
            Environment.Exit(0);
        }

        [Command("ping"), Description("Muestra el ping de Yumiko.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Color = funciones.GetColor(),
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`"
            }).ConfigureAwait(false);
        }

        [Command("invite"), Aliases("invitar"), Description("Muestra el link para invitar a Yumiko a un servidor.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
            await ctx.Message.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
        }

        [Command("test"), RequireOwner]
        public async Task Test(CommandContext ctx)
        {
            var guild = await ctx.Client.GetGuildAsync(713809173573271613);
            var channel = guild.GetChannel(781679685838569502);
            await channel.SendMessageAsync("prueba");
        }
    }
}
