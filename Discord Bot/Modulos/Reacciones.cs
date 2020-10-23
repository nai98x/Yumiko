using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Reacciones : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        /*
        [Command("fuck"), RequireNsfw]
        public async Task Fuck(CommandContext ctx, DiscordUser usuario = null)
        {
            DiscordMember user = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            string comando = "fuck";
            string titulo = "Fuck";
            string descripcion;
            if (usuario == null)
            {
                string[] opciones = new string[]
                {
                    ""
                };
                descripcion = $"";
                await Reaccionar(ctx, opciones, comando, titulo, descripcion);
            }
            else
            {
                string[] opciones = new string[]
                {
                    ""
                };
                descripcion = $"";
                await Reaccionar(ctx, opciones, comando, titulo, descripcion);
            }
            await ctx.Message.DeleteAsync("Auto borrado de yumiko");
        }
        */

        [Command("fuck"), RequireNsfw]
        public async Task Fuck(CommandContext ctx, DiscordMember usuario = null)
        {
            DiscordMember user = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            string comando = "fuck";
            string descripcion;
            if (usuario == null)
            {
                string[] opciones = new string[]
                {
                    ""
                };
                descripcion = $"";
                await Reaccionar(ctx, opciones, comando, descripcion);
            }
            else
            {
                string[] opciones = new string[]
                {
                    "https://i.imgur.com/LUxZdZV.gif",
                    "https://i.imgur.com/axBHueH.gif",
                    "https://i.imgur.com/Bq60h6f.gif",
                    "https://i.imgur.com/zVGiJBm.gif"
                };
                descripcion = $"**{user.DisplayName}** se folló a **{usuario.DisplayName}** OwO";
                await Reaccionar(ctx, opciones, comando, descripcion);
            }
            await ctx.Message.DeleteAsync("Auto borrado de yumiko");
        }

        public async Task Reaccionar(CommandContext ctx, string[] opciones, string comando, string descripcion)
        {
            Random rnd = new Random();
            string imgElegida = opciones[rnd.Next(opciones.Length - 1)];
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder { 
                Description = descripcion,
                Footer = funciones.GetFooter(ctx, comando),
                Color = new DiscordColor(78, 63, 96),
                ImageUrl = imgElegida
            }).ConfigureAwait(false);
        }
    }
}
