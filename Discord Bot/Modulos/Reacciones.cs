using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Reacciones : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("fuck"), RequireNsfw]
        public async Task Fuck(CommandContext ctx, DiscordMember usuario = null)
        {
            DiscordMember self = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            string comando = "fuck";
            string descripcion;
            if (usuario == null || usuario.Id == ctx.User.Id)
            {
                string[] opciones = new string[]
                {
                    "https://i.imgur.com/s9ZzGvz.gif"
                };
                descripcion = $"{self.Mention}, no puedes hacerte eso a ti mismo.";
                await Reaccionar(ctx, opciones, comando, descripcion);
            }
            else
            {
                string[] opciones = new string[]
                {
                    "https://i.imgur.com/LUxZdZV.gif",
                    "https://i.imgur.com/axBHueH.gif",
                    "https://i.imgur.com/Bq60h6f.gif",
                    "https://i.imgur.com/zVGiJBm.gif",
                    "https://i.imgur.com/EjCnwCr.gif",
                    "https://i.imgur.com/JgC2PGx.gif",
                    "https://i.imgur.com/PQNbcBC.gif",
                    "https://i.imgur.com/1gqgFqi.gif",
                    "https://i.imgur.com/uSu3SNi.gif",
                    "https://i.imgur.com/lg5MhsK.gif"
                };
                descripcion = $"{self.Mention} se folló a {usuario.Mention} OwO";
                await Reaccionar(ctx, opciones, comando, descripcion);
            }
            await ctx.Message.DeleteAsync("Auto borrado de yumiko");
        }

        [Command("blowjob"), Aliases("suck"), RequireNsfw]
        public async Task Blowjob(CommandContext ctx, DiscordMember usuario = null)
        {
            DiscordMember self = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            string comando = "blowjob";
            string descripcion;
            if (usuario == null || usuario.Id == ctx.User.Id)
            {
                string[] opciones = new string[]
                {
                    "https://i.imgur.com/s9ZzGvz.gif"
                };
                descripcion = $"{self.Mention}, no puedes hacerte eso a ti mismo.";
                await Reaccionar(ctx, opciones, comando, descripcion);
            }
            else
            {
                string[] opciones = new string[]
                {
                    "https://i.imgur.com/6QGq5kE.gif",
                    "https://i.imgur.com/W4msRFP.gif",
                    "https://i.imgur.com/eP5Soa4.gif",
                    "https://i.imgur.com/Op23bBq.gif",
                    "https://i.imgur.com/8Vrb7J5.gif",
                    "https://i.imgur.com/DSNciLo.gif",
                    "https://i.imgur.com/aa4jgyn.gif",
                    "https://i.imgur.com/Blrl61W.gif",
                    "https://i.imgur.com/6LgfOA0.gif",
                    "https://i.imgur.com/IadM6d5.gif"
                };
                descripcion = $"{self.Mention} le chupó el pene a {usuario.Mention} OwO";
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
