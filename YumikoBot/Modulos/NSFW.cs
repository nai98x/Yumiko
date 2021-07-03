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

        [Command("fuck"), Description("Tienes sexo con alguien")]
        public async Task Fuck(CommandContext ctx, [Description("El usuario que te quieres follar")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130213346934834);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que se lo follen",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync($"", embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** se folló a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("cum"), Description("Te vienes")]
        public async Task Cum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GCum(ctx, usuario);
            else
                await BCum(ctx, usuario);
        }

        [Command("gcum"), Description("Te vienes (vagina)")]
        public async Task GCum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordMember usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130231152803882);
                string titulo;
                if (usuario == null)
                    titulo = $"**{ctx.Member.DisplayName}** se ha venido";
                else
                    titulo = $"**{ctx.Member.DisplayName}** se vino en **{usuario.DisplayName}**";
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
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }

        [Command("bcum"), Description("Te vienes (pene)")]
        public async Task BCum(CommandContext ctx, [Description("El usuario al que le quieres acabar")] DiscordMember usuario = null)
        {
            if (usuario == null || (usuario != null && ctx.User.Id != usuario.Id))
            {
                Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130246767280180);
                string titulo;
                if (usuario == null)
                    titulo = $"**{ctx.Member.DisplayName}** se ha venido";
                else
                    titulo = $"**{ctx.Member.DisplayName}** se vino en **{usuario.DisplayName}**";
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
                var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }

        [Command("boobjob"), Description("Le haces una rusa a alguien")]
        public async Task Boobjob(CommandContext ctx, [Description("El usuario que le quieres hacer una rusa")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130262567485483);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que le hagan una rusa",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** le hizo una rusa a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("masturbate"), Description("Te masturbas")]
        public async Task Masturbate(CommandContext ctx, [Description("El usuario al que quieres masturbar")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GMasturbate(ctx, usuario);
            else
                await BMasturbate(ctx, usuario);
        }

        [Command("gmasturbate"), Description("Te masturbas (vagina)")]
        public async Task GMasturbate(CommandContext ctx, [Description("El usuario al que quieres masturbar")] DiscordMember usuario = null)
        {
            Imagen elegida; 
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130300458565672);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** se está masturbando",
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
                    Title = $"**{ctx.Member.DisplayName}** está masturbando a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bmasturbate"), Description("Te masturbas (pene)")]
        public async Task BMasturbate(CommandContext ctx, [Description("El usuario al que quieres masturbar")] DiscordMember usuario = null)
        {
            Imagen elegida;
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130322554945567); // Cambiar por el solo, cuando tenga contenido
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** se está masturbando",
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
                    Title = $"**{ctx.Member.DisplayName}** está masturbando a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("suck"), Description("Se la chupas a alguien")]
        public async Task Suck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordMember usuario = null)
        {
            Random rnd = new();
            if (rnd.NextDouble() >= 0.5)
                await GSuck(ctx, usuario);
            else
                await BSuck(ctx, usuario);
        }

        [Command("gsuck"), Description("Se la chupas a alguien (con vagina)")]
        public async Task GSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130366343348234);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que se la chupen",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** se la chupó a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bsuck"), Description("Se la chupas a alguien (con pene)")]
        public async Task BSuck(CommandContext ctx, [Description("El usuario que se la quieres chupar")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816130449679319041);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que se la chupen",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** se la chupó a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yaoi"), Description("Sexo entre hombres")]
        public async Task Yaoi(CommandContext ctx, [Description("Tu compañero")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152519256178750);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está teniendo pensamientos yaoi",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** tiene una fantasía yaoi con **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("yuri"), Description("Sexo entre mujeres")]
        public async Task Yuri(CommandContext ctx, [Description("Tu compañera")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152538906230795);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está teniendo pensamientos yuri",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** tiene una fantasía yuri con **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("futa"), Description("Sexo entre futanaris")]
        public async Task Futa(CommandContext ctx, [Description("Tu compañer@")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152813917962281);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está teniendo pensamientos futas",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** tiene una fantasía futa con **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("bdsm"), Description("Sexo con alguien mediante ataduras")]
        public async Task Bdsm(CommandContext ctx, [Description("Tu compañero")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152852023476224);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está teniendo pensamientos BDSM",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** tiene una fantasía BDSM con **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("azotes"), Description("Sexo con alguien mediante el uso de latigos")]
        public async Task Azotes(CommandContext ctx, [Description("Tu compañero")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152880862986260);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que lo azoten",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está azotando a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("pisar"), Description("Pisas a otro")]
        public async Task Pisotones(CommandContext ctx, [Description("Tu compañer@")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152907690672188);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que lo pisen",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está pisando a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("trio"), Description("Tienes un trio con otros dos usuarios")]
        public async Task Trio(CommandContext ctx, [Description("Tu compañero")] DiscordMember usuario1 = null, [Description("Tu compañero")] DiscordMember usuario2 = null)
        {
            if (usuario1 == null || usuario2 == null)
            {
                var msg = await ctx.RespondAsync("Debes mencionar a dos personas");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
            else
            {
                if (ctx.User.Id == usuario1.Id || ctx.User.Id == usuario2.Id)
                {
                    var msg = await ctx.RespondAsync("No puedes hacerte eso a ti mismo");
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
                else
                {
                    Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816152926535417906);
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = $"**{ctx.Member.DisplayName}** tiene un trio con **{usuario1.DisplayName}** y **{usuario2.DisplayName}**",
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        ImageUrl = elegida.Url
                    });
                }
            }
        }

        [Command("orgia"), Description("Provocas una orgia")]
        public async Task Orgia(CommandContext ctx, params DiscordMember[] miembros)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154197790687263);
            if(miembros.Length == 0)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** ha iniciado una orgia en el servidor",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                string mentions = "Con ";
                foreach (var miembro in miembros)
                {
                    mentions += $"**{miembro.DisplayName}**, ";
                }
                mentions = mentions.Remove(mentions.Length - 2);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** ha iniciado una orgia en el servidor",
                    Description = mentions,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("gangbang"), Description("Te hacen un gangbang")]
        public async Task Gangbang(CommandContext ctx, [Description("A quien quieres que se lo hagan")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154217864888340);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Title = $"Le están haciendo un ganbang a **{ctx.Member.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que le hagan un ganbang a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("footjob"), Description("Masturbas a otro con los pies")]
        public async Task Footjob(CommandContext ctx, [Description("Tu compañer@")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154242997551104);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** quiere que le hagan footjob",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** masturba con su pie a **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("erection"), Description("Tienes una ereccion")]
        public async Task Erection(CommandContext ctx, [Description("Tu compañer@")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154258503630918);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** ha tenido una erección",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{usuario.DisplayName}** le ha provocado una erección a **{ctx.Member.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("cosplay"), Description("Haces cosplay")]
        public async Task Cosplay(CommandContext ctx, [Description("Tu compañer@")] DiscordMember usuario = null)
        {
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 816154286551597086);
            if (usuario == null || ctx.User.Id == usuario.Id)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está haciendo cosplay para hacer cosas impuras",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.DisplayName}** está haciendo cosplay para hacer cosas impuras con **{usuario.DisplayName}**",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    ImageUrl = elegida.Url
                });
            }
        }

        [Command("fakensfw"), Description("Fake NSFW"), RequireOwner, Hidden]
        public async Task FakeNSFW(CommandContext ctx, [Description("Usuario fake")] DiscordMember usuarioFake, [Description("Usuario destinatario")] DiscordMember usuario)
        {
            var interactivity = ctx.Client.GetInteractivity();

            DiscordComponentEmoji emote = new(DiscordEmoji.FromName(ctx.Client, ":game_die:"));
            DiscordButtonComponent button0 = new(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent button1 = new(ButtonStyle.Primary, "1", "fuck");
            DiscordButtonComponent button2 = new(ButtonStyle.Primary, "2", "gcum");
            DiscordButtonComponent button3 = new(ButtonStyle.Primary, "3", "bcum");
            DiscordButtonComponent button4 = new(ButtonStyle.Primary, "4", "boobjob");
            DiscordButtonComponent button5 = new(ButtonStyle.Primary, "5", "gmasturbate");
            DiscordButtonComponent button6 = new(ButtonStyle.Primary, "6", "bmasturbate");
            DiscordButtonComponent button7 = new(ButtonStyle.Primary, "7", "gsuck");
            DiscordButtonComponent button8 = new(ButtonStyle.Primary, "8", "bsuck");
            DiscordButtonComponent button9 = new(ButtonStyle.Primary, "9", "yaoi");
            DiscordButtonComponent button10 = new(ButtonStyle.Primary, "10", "yuri");
            DiscordButtonComponent button11 = new(ButtonStyle.Primary, "11", "futa");
            DiscordButtonComponent button12 = new(ButtonStyle.Primary, "12", "bdsm");
            DiscordButtonComponent button13 = new(ButtonStyle.Primary, "13", "azotes");
            DiscordButtonComponent button14 = new(ButtonStyle.Primary, "14", "pisar");
            DiscordButtonComponent button15 = new(ButtonStyle.Primary, "15", "footjob");
            DiscordButtonComponent button16 = new(ButtonStyle.Primary, "16", "erection");
            DiscordButtonComponent button17 = new(ButtonStyle.Primary, "17", "cosplay");

            DiscordMessageBuilder mensaje = new()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elije el tipo de juego",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };

            mensaje.AddComponents(button0, button1, button2, button3, button4);
            mensaje.AddComponents(button5, button6, button7, button8, button9);
            mensaje.AddComponents(button10, button11, button12, button13, button14);
            mensaje.AddComponents(button15, button16, button17);

            DiscordMessage msgElegir = await mensaje.SendAsync(ctx.Channel);
            var interComando = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await msgElegir.DeleteAsync("Auto borrado de Yumiko");
            if (!interComando.TimedOut)
            {
                var resultComando = interComando.Result;
                string comando = resultComando.Id;
                if (comando == "0")
                {
                    var dificultadNum = funciones.GetNumeroRandom(1, 17);
                    comando = dificultadNum.ToString();
                }
                var cNext = ctx.Client.GetCommandsNext();
                switch (comando)
                {
                    case "1":
                        await Fuck(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("fuck", out _)), usuario);
                        break;
                    case "2":
                        await GCum(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("gcum", out _)), usuario);
                        break;
                    case "3":
                        await BCum(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("bcum", out _)), usuario);
                        break;
                    case "4":
                        await Boobjob(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("boobjob", out _)), usuario);
                        break;
                    case "5":
                        await GMasturbate(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("gmasturbate", out _)), usuario);
                        break;
                    case "6":
                        await BMasturbate(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("bmasturbate", out _)), usuario);
                        break;
                    case "7":
                        await GSuck(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("gsuck", out _)), usuario);
                        break;
                    case "8":
                        await BSuck(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("bsuck", out _)), usuario);
                        break;
                    case "9":
                        await Yaoi(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("yaoi", out _)), usuario);
                        break;
                    case "10":
                        await Yuri(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("yuri", out _)), usuario);
                        break;
                    case "11":
                        await Futa(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("futa", out _)), usuario);
                        break;
                    case "12":
                        await Bdsm(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("bdsm", out _)), usuario);
                        break;
                    case "13":
                        await Azotes(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("azotes", out _)), usuario);
                        break;
                    case "14":
                        await Pisotones(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("pisotones", out _)), usuario);
                        break;
                    case "15":
                        await Footjob(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("footjob", out _)), usuario);
                        break;
                    case "16":
                        await Erection(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("erection", out _)), usuario);
                        break;
                    case "17":
                        await Cosplay(cNext.CreateFakeContext(usuarioFake, ctx.Channel, "", ctx.Prefix, cNext.FindCommand("cosplay", out _)), usuario);
                        break;
                }
            }
        }
    }
}