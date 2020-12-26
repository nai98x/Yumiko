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
    [RequireNsfw]
    public class NSFW : BaseCommandModule
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
                    if(lista.Count > 0)
                    {
                        var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor(),
                            Description = $"{ctx.User.Mention} se folló a {usuario}",
                            ImageUrl = elegida.Url
                        });
                    }
                }
            }
        }

        [Command("cum"), Description("Te vienes")]
        public async Task Cum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordUser usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                var lista = imagenes.GetImagenes("cum");
                if(lista.Count > 0)
                {
                    var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                    string titulo;
                    if (usuario == null)
                        titulo = $"{ctx.User.Mention} se ha venido";
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
            }
            else
            {
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("boobjob"), Description("Le haces una rusa a alguien")]
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
                    if(lista.Count > 0)
                    {
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
        }

        [Command("gmasturbate"), Description("Te masturbas (chica)")]
        public async Task GMasturbate(CommandContext ctx)
        {
            var lista = imagenes.GetImagenes("gmasturbate");
            if(lista.Count > 0)
            {
                var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Description = $"{ctx.User.Mention} se está tocando",
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bmasturbate"), Description("Te masturbas (chico)")]
        public async Task BMasturbate(CommandContext ctx)
        {
            var lista = imagenes.GetImagenes("bmasturbate");
            if(lista.Count > 0)
            {
                var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Description = $"{ctx.User.Mention} se está tocando",
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gsuck"), Description("Se la chupas a alguien (chica)")]
        public async Task GSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordUser usuario = null)
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
                    var lista = imagenes.GetImagenes("gsuck");
                    if (lista.Count > 0)
                    {
                        var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor(),
                            Description = $"{ctx.User.Mention} se la chupó a {usuario}",
                            ImageUrl = elegida.Url
                        });
                    }
                }
            }
        }

        [Command("bsuck"), Description("Se la chupas a alguien (chico)")]
        public async Task BSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordUser usuario = null)
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
                    var lista = imagenes.GetImagenes("bsuck");
                    if (lista.Count > 0)
                    {
                        var elegida = lista[funciones.GetNumeroRandom(0, lista.Count - 1)];
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor(),
                            Description = $"{ctx.User.Mention} se la chupó a {usuario}",
                            ImageUrl = elegida.Url
                        });
                    }
                }
            }
        }

    }
}
