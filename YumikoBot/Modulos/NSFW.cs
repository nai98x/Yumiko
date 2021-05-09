using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    [RequireNsfw]
    public class NSFW : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("fuck"), Description("Tienes sexo con alguien")]
        public async Task Fuck(CommandContext ctx, [Description("El usuario que te quieres follar")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130213346934834);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que se lo follen", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} se folló a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gcum"), Description("Te vienes (vagina)")]
        public async Task GCum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordUser usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130231152803882);
                string titulo;
                if (usuario == null)
                    titulo = $"{ctx.User.Mention} se ha venido";
                else
                    titulo = $"{ctx.User.Mention} se vino en {usuario.Mention}";
                await ctx.RespondAsync(titulo, embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember) == DSharpPlus.Permissions.ManageMessages)
                {
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
            }
        }

        [Command("bcum"), Description("Te vienes (pene)")]
        public async Task BCum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordUser usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130246767280180);
                string titulo;
                if (usuario == null)
                    titulo = $"{ctx.User.Mention} se ha venido";
                else
                    titulo = $"{ctx.User.Mention} se vino en {usuario.Mention}";
                await ctx.RespondAsync(titulo, embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember) == DSharpPlus.Permissions.ManageMessages)
                {
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
            }
        }

        [Command("boobjob"), Description("Le haces una rusa a alguien")]
        public async Task Boobjob(CommandContext ctx, [Description("El usuario que le quieres hacer una rusa")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130262567485483);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que le hagan una rusa", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} le hizo una rusa a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gmasturbate"), Description("Te masturbas (vagina)")]
        public async Task GMasturbate(CommandContext ctx, [Description("El usuario al que quieres masturbar")] DiscordUser usuario = null)
        {
            Imagen elegida; 
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130300458565672);
                await ctx.RespondAsync($"{ctx.User.Mention} se está masturbando", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 823001635751854130);
                await ctx.RespondAsync($"{ctx.User.Mention} está masturbando a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bmasturbate"), Description("Te masturbas (pene)")]
        public async Task BMasturbate(CommandContext ctx, [Description("El usuario al que quieres masturbar")] DiscordUser usuario = null)
        {
            Imagen elegida;
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130322554945567); // Cambiar por el solo, cuando tenga contenido
                await ctx.RespondAsync($"{ctx.User.Mention} se está masturbando", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130322554945567);
                await ctx.RespondAsync($"{ctx.User.Mention} está masturbando a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gsuck"), Description("Se la chupas a alguien (con vagina)")]
        public async Task GSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130366343348234);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que se la chupen", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} se la chupó a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bsuck"), Description("Se la chupas a alguien (con pene)")]
        public async Task BSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130449679319041);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que se la chupen", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} se la chupó a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yaoi"), Description("Sexo entre hombres")]
        public async Task Yaoi(CommandContext ctx, [Description("Tu compañero")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152519256178750);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está teniendo pensamientos yaoi", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} tiene una fantasía yaoi con {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yuri"), Description("Sexo entre mujeres")]
        public async Task Yuri(CommandContext ctx, [Description("Tu compañera")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152538906230795);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está teniendo pensamientos yuri", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} tiene una fantasía yuri con {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("futa"), Description("Sexo entre futanaris")]
        public async Task Futa(CommandContext ctx, [Description("Tu compañer@")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152813917962281);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está teniendo pensamientos futas", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} tiene una fantasía futa con {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bdsm"), Description("Sexo con alguien mediante ataduras")]
        public async Task Bdsm(CommandContext ctx, [Description("Tu compañero")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152852023476224);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está teniendo pensamientos BDSM", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} tiene una fantasía BDSM con {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("azotes"), Description("Sexo con alguien mediante el uso de latigos")]
        public async Task Azotes(CommandContext ctx, [Description("Tu compañero")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152880862986260);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que lo azoten", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está azotando a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("pisar"), Description("Pisas a otro")]
        public async Task Pisotones(CommandContext ctx, [Description("Tu compañer@")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152907690672188);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que lo pisen", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está pisando a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("trio"), Description("Tienes un trio con otros dos usuarios")]
        public async Task Trio(CommandContext ctx, [Description("Tu compañero")] DiscordUser usuario1 = null, [Description("Tu compañero")] DiscordUser usuario2 = null)
        {
            if (usuario1 == null || usuario2 == null)
            {
                var msg = await ctx.RespondAsync("Debes mencionar a dos personas");
                if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember) == DSharpPlus.Permissions.ManageMessages)
                {
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                }
                    
            }
            else
            {
                if (ctx.User.Id == usuario1.Id || ctx.User.Id == usuario2.Id)
                {
                    var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                    if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember) == DSharpPlus.Permissions.ManageMessages)
                    {
                        await Task.Delay(3000);
                        await msg.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                else
                {
                    Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152926535417906);
                    await ctx.RespondAsync($"{ctx.User.Mention} tiene un trio con {usuario1.Mention} y {usuario2.Mention}", embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        ImageUrl = elegida.Url
                    });
                }
            }
        }

        [Command("orgia"), Description("Provocas una orgia")]
        public async Task Orgia(CommandContext ctx)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154197790687263);
            await ctx.RespondAsync($"{ctx.User.Mention} ha iniciado una orgia en el servidor", embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                ImageUrl = elegida.Url
            });
        }

        [Command("gangbang"), Description("Te hacen un gangbang")]
        public async Task Gangbang(CommandContext ctx, [Description("A quien quieres que se lo hagan")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154217864888340);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"Le están haciendo un ganbang a {ctx.User.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que le hagan un ganbang a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("footjob"), Description("Masturbas a otro con los pies")]
        public async Task Footjob(CommandContext ctx, [Description("Tu compañer@")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154242997551104);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} quiere que le hagan footjob", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} masturba con su pie a {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("erection"), Description("Tienes una ereccion")]
        public async Task Erection(CommandContext ctx, [Description("Tu compañer@")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154258503630918);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} ha tenido una erección", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{usuario.Mention} le ha provocado una erección a {ctx.User.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("cosplay"), Description("Haces cosplay")]
        public async Task Cosplay(CommandContext ctx, [Description("Tu compañer@")] DiscordUser usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154286551597086);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está haciendo cosplay para hacer cosas impuras", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"{ctx.User.Mention} está haciendo cosplay para hacer cosas impuras con {usuario.Mention}", embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }
    }
}