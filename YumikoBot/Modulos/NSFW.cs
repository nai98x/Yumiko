using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    [RequireNsfw]
    public class NSFW : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [Command("fuck"), Description("Fuck with someone")]
        public async Task Fuck(CommandContext ctx, [Description("The user you want to fuck")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130213346934834);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** he wants to be fucked",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"", embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** fucked **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("cum"), Description("You came")]
        public async Task Cum(CommandContext ctx, [Description("The user you want to cum")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GCum(ctx, usuario);
            else
                await BCum(ctx, usuario);
        }

        [Command("gcum"), Description("You came (pussy)")]
        public async Task GCum(CommandContext ctx, [Description("You user yopu want to came")] DiscordMember usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130231152803882);
                string titulo;
                if (usuario == null)
                    titulo = $"**{ctx.Member.DisplayName}** just came";
                else
                    titulo = $"**{ctx.Member.DisplayName}** just came inside **{usuario.DisplayName}**";
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = titulo,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                var msg = await ctx.RespondAsync("You can't do that to yourself");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }

        [Command("bcum"), Description("You came (dick)")]
        public async Task BCum(CommandContext ctx, [Description("You user yopu want to came")] DiscordMember usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130246767280180);
                string titulo;
                if (usuario == null)
                    titulo = $"**{ctx.Member.DisplayName}** just came";
                else
                    titulo = $"**{ctx.Member.DisplayName}** just came inside **{usuario.DisplayName}**";
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = titulo,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                var msg = await ctx.RespondAsync("You can't do that to yourself");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }

        [Command("boobjob"), Description("Do a boobjob to someone")]
        public async Task Boobjob(CommandContext ctx, [Description("The user you want to do a boobjob")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130262567485483);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants to do a boobjob",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** just made a boobjob to **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("masturbate"), Description("You masturbate")]
        public async Task Masturbate(CommandContext ctx, [Description("The user you want to masturbate")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GMasturbate(ctx, usuario);
            else
                await BMasturbate(ctx, usuario);
        }

        [Command("gmasturbate"), Description("You masturbate (pussy)")]
        public async Task GMasturbate(CommandContext ctx, [Description("The user you want to masturbate")] DiscordMember usuario = null)
        {
            Imagen elegida; 
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130300458565672);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is masturbating",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 823001635751854130);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is jerking off **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bmasturbate"), Description("You masturbate (dick)")]
        public async Task BMasturbate(CommandContext ctx, [Description("The user you want to masturbate")] DiscordMember usuario = null)
        {
            Imagen elegida;
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130322554945567); // Cambiar por el solo, cuando tenga contenido
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is masturbating",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130322554945567);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is jerking off **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("suck"), Description("You suck someone's genitals")]
        public async Task Suck(CommandContext ctx, [Description("The user you want to suck")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GSuck(ctx, usuario);
            else
                await BSuck(ctx, usuario);
        }

        [Command("gsuck"), Description("You suck someone's vagina")]
        public async Task GSuck(CommandContext ctx, [Description("The user you want to suck")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130366343348234);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants a blowjob",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** suck **{usuario.DisplayName}'s pussy**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bsuck"), Description("You suck someone's dick")]
        public async Task BSuck(CommandContext ctx, [Description("The user you want to suck")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130449679319041);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants a blowjob",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** suck **{usuario.DisplayName}'s dick**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yaoi"), Description("Sexo entre hombres")]
        public async Task Yaoi(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152519256178750);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is having yaoi fantasies",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** haves a yaoi fantasy with **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yuri"), Description("Sexo entre mujeres")]
        public async Task Yuri(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152538906230795);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is having yuri fantasies",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** haves a yuri fantasy with **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("futa"), Description("Futanari sex")]
        public async Task Futa(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152813917962281);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is having futanari fatasies",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** haves a futanari fantasy with **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bdsm"), Description("Bondage sex with someone")]
        public async Task Bdsm(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152852023476224);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is having a bondage fantasy",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is having bondage sex with **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("whipping"), Description("Sex with someone by using whips")]
        public async Task Azotes(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152880862986260);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants to be spanked",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is spanking **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("step"), Description("Step someone")]
        public async Task Pisotones(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152907690672188);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants to be stepped on",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is stepping on **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("threesome"), Description("Have a threesome with two other users")]
        public async Task Trio(CommandContext ctx, [Description("Tu compañero")] DiscordMember usuario1 = null, [Description("Tu compañero")] DiscordMember usuario2 = null)
        {
            if (usuario1 == null || usuario2 == null)
            {
                var msg = await ctx.RespondAsync("You have to mention two other users");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
            else
            {
                if (ctx.User.Id == usuario1.Id || ctx.User.Id == usuario2.Id)
                {
                    var msg = await ctx.RespondAsync("You can do this to yourself");
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
                else
                {
                    Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152926535417906);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = $"**{ctx.Member.DisplayName}** have a threesome with **{usuario1.DisplayName}** and **{usuario2.DisplayName}**",
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        ImageUrl = elegida.Url
                    });
                }
            }
        }

        [Command("orgy"), Description("You provoke an orgy")]
        public async Task Orgia(CommandContext ctx, params DiscordMember[] miembros)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154197790687263);
            if(miembros.Length == 0)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** has started an orgy on the guild",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                string mentions = "With ";
                foreach (var miembro in miembros)
                {
                    mentions += $"**{miembro.DisplayName}**, ";
                }
                mentions = mentions.Remove(mentions.Length - 2);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** has started an orgy on the guild",
                    Description = mentions,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gangbang"), Description("Do a gangbang")]
        public async Task Gangbang(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154217864888340);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Title = $"The guild members are doing a ganbang to **{ctx.Member.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants to gangbang **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("footjob"), Description("You masturbate another with your feet")]
        public async Task Footjob(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154242997551104);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** wants to get a footjob",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** footjobs **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("erection"), Description("Have an erection")]
        public async Task Erection(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154258503630918);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** just had an erection",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{usuario.DisplayName}** has caused an erection to **{ctx.Member.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("cosplay"), Description("Fuck with cosplay")]
        public async Task Cosplay(CommandContext ctx, [Description("User")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154286551597086);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is cosplaying to do impure things",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** is cosplaying to do impure things with **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }
    }
}