using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using GraphQL;
using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using YumikoBot.Data_Access_Layer;

namespace Discord_Bot.Modulos
{
    public class Reacciones : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly Imagenes imagenes = new Imagenes();

        [Command("fuck"), Description("Tienes sexo con alguien")]
        public async Task Fuck(CommandContext ctx, [Description("El usuario que te quieres follar")] DiscordUser usuario = null)
        {
            if (usuario == null)
            {
                var msg = await ctx.RespondAsync("Debes mencionar a alguien");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
            }
            else
            {
                if (ctx.User.Id == usuario.Id)
                {
                    var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
                else
                {
                    var lista = imagenes.GetImagenes("fuck");
                    var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Title = $"{ctx.User.Mention} se folló a {usuario}",
                        ImageUrl = elegida.Url
                    });
                }
            }
        }

        [Command("cum"), Description("Te vienes")]
        public async Task Cum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordUser usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                var lista = imagenes.GetImagenes("cum");
                var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                string titulo;
                if (usuario == null)
                    titulo= $"{ctx.User.Mention} se ha venido";
                else
                    titulo = $"{ctx.User.Mention} se vino en {usuario.Mention}";
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Description = titulo,
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("boobjob"), Description("Tienes sexo con alguien")]
        public async Task Boobjob(CommandContext ctx, [Description("El usuario que le quieres hacer una rusa")] DiscordUser usuario = null)
        {
            if (usuario == null)
            {
                var msg = await ctx.RespondAsync("Debes mencionar a alguien");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
            }
            else
            {
                if (ctx.User.Id == usuario.Id)
                {
                    var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
                else
                {
                    var lista = imagenes.GetImagenes("boobjob");
                    var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Description = $"{ctx.User.Mention} le hizo una rusa a {usuario.Mention}",
                        ImageUrl = elegida.Url
                    });
                }
            }
        }

        [Command("addReaccion"), Aliases("addReaction"), Description("Agrega una reaccion"), RequireOwner]
        public async Task AddReaccion(CommandContext ctx, [Description("Imagen de la reaccion")]string url)
        {
            var interactivity = ctx.Client.GetInteractivity();
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
    }
}
