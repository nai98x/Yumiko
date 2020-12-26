using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Configuration;
using System.Threading.Tasks;
using YumikoBot.Data_Access_Layer;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("reiniciar"), Aliases("restart"), RequireOwner, Description("Se reinicia Yumiko.")]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar"), RequireOwner, Description("Se apaga Yumiko.")]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
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
        }

        [Command("addReaccion"), Aliases("addReaction"), Description("Agrega una reaccion"), RequireOwner]
        public async Task AddReaccion(CommandContext ctx, [Description("Imagen de la reaccion")] string url)
        {
            var interactivity = ctx.Client.GetInteractivity();
            Imagenes imagenes = new Imagenes();
            var msgImagen = await ctx.RespondAsync("Indica el comando para la reaccion");
            var interact = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!interact.TimedOut)
            {
                string comando = interact.Result.Content;
                imagenes.AddImagen(url, comando);
                var msg = await ctx.RespondAsync("Reaccion añadida");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
                await interact.Result.DeleteAsync("Auto borrado de Yumiko");
                await msgImagen.DeleteAsync("Auto borrado de Yumiko");
            }
            else
            {
                var msg = await ctx.RespondAsync("Tiempo agotado esperando el comando");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
                await msgImagen.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("servers"), RequireOwner]
        public async Task Servers(CommandContext ctx)
        {
            string servers = "";
            var guilds = ctx.Client.Guilds.Values;
            foreach (var guild in guilds)
            {
                servers += 
                    $"\n**{guild.Name}**\n" +
                    $"  - **Id**: {guild.Id}\n" +
                    $"  - **Fecha que entró Yumiko**: {guild.JoinedAt}\n" +
                    $"  - **Miembros**: {guild.MemberCount}\n";
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "Servidores de Yumiko",
                Description = servers,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            });
        }

        [Command("eliminarmensaje"), RequireOwner]
        public async Task BorrarMensajePropio(CommandContext ctx, ulong id)
        {
            var msg = await ctx.Channel.GetMessageAsync(id);
            if(msg != null && msg.Author.Id == ctx.Client.CurrentUser.Id)
            {
                await ctx.Channel.DeleteMessageAsync(msg, "Auto borrado de Yumiko");
            }
            else
            {
                var mensaje = await ctx.RespondAsync("Solo puedo borrar mis propios mensajes");
                await Task.Delay(3000);
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
            }
        }
    }
}
