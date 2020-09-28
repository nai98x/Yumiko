using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Memes : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("waifu")]
        [Description("Te digo si soy tu waifu")]
        public async Task Waifu(CommandContext ctx, DiscordMember miembro = null)
        {
            string nombre;
            if (miembro == null)
                nombre = ctx.Member.DisplayName;
            else
                nombre = miembro.DisplayName;

            Random rnd = new Random();
            int waifuLevel = rnd.Next(101);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.TriggerTypingAsync();
            if (waifuLevel < 25)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Red,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe pego un tiro antes de tocarte.",
                    ImageUrl = "https://i.imgur.com/BOxbruw.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 25 && waifuLevel < 50)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Orange,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe das asquito, mejor me alejo de vos.",
                    ImageUrl = "https://i.imgur.com/ys2HoiL.jpg"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 50 && waifuLevel < 75)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Yellow,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nNo estás mal, pero no tenes posibilidades conmigo.",
                    ImageUrl = "https://i.imgur.com/jCfDrGa.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 75 && waifuLevel < 100)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Green,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nSoy tu waifu, podes hacer lo que quieras conmigo.",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel == 100)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Blue,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\n.Estoy completamente enamorada de ti, ¿cuándo nos casamos?",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png"
                    //ImageUrl = "https://i.imgur.com/b5g1LEP.png" quitada por NSFW
                }).ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("husbando")]
        [Description("Elijo mi husbando")]
        public async Task Husbando(CommandContext ctx)
        {
            Random rnd = new Random();
            var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false);
            DiscordMember elegido = miembros.ElementAt(rnd.Next(miembros.Count() - 1)).Value;
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = new DiscordColor(78, 63, 96),
                Title = "Husbando",
                Description = $"Mi husbando es: **{elegido.DisplayName} ({elegido.Username}#{elegido.Discriminator})** 💘",
                ImageUrl = elegido.AvatarUrl
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
        }

        [Command("ship")]
        [Description("Elijo a tu shippeo")]
        public async Task Ship(CommandContext ctx, DiscordUser usuario = null)
        {
            Random rnd = new Random();
            DiscordRole kohai = ctx.Guild.GetRole(713484136001700042);
            DiscordRole senpai = ctx.Guild.GetRole(713484281950765138);
            var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false && (x.Value.Roles.Contains(kohai) || x.Value.Roles.Contains(senpai)));

            DiscordMember elegido = miembros.ElementAt(rnd.Next(miembros.Count() - 1)).Value;

            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            string shipeoUsr;
            if(usuario == null) 
            {
                DiscordMember ctxMiembro = await ctx.Guild.GetMemberAsync(ctx.User.Id);
                shipeoUsr = ctxMiembro.DisplayName;
            }
            else 
            {
                DiscordMember ctxMiembro = await ctx.Guild.GetMemberAsync(usuario.Id);
                shipeoUsr = ctxMiembro.DisplayName;
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = new DiscordColor(78, 63, 96),
                Title = "Shippeo",
                Description = $"Shippeo a **{shipeoUsr}** con **{elegido.DisplayName}** 💘",
                ImageUrl = funciones.GetImagenRandomShip()
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
        }

        [Command("ooc")]
        [Description("Out of Context")]
        [RequireNsfw]
        public async Task OOC(CommandContext ctx)
        {
            DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(748315008131268693);
            if (discordOOC == null)
            {
                await ctx.RespondAsync("Error al obtener servidor **AniList ESP OOC**").ConfigureAwait(false);
                return;
            }
            DiscordChannel channel = discordOOC.GetChannel(748315008131268698);
            if (channel == null)
            {
                await ctx.RespondAsync("Error al obtener canal **#capturas** del servidor **AniList ESP OOC**").ConfigureAwait(false);
                return;
            }
            IReadOnlyList<DiscordMessage> mensajes = await channel.GetMessagesAsync();
            List<DiscordMessage> msgs = mensajes.ToList<DiscordMessage>();
            int cntMensajes = msgs.Count();
            DiscordMessage last = msgs.LastOrDefault();
            while (cntMensajes == 100)
            {
                var mensajesAux = await channel.GetMessagesBeforeAsync(last.Id);

                cntMensajes = mensajesAux.Count();
                last = mensajesAux.LastOrDefault();

                foreach (DiscordMessage mensaje in mensajesAux){
                    msgs.Add(mensaje);
                }
            }
            List<Imagen> opciones = new List<Imagen>();
            foreach(DiscordMessage msg in msgs)
            {
                var att = msg.Attachments.FirstOrDefault();
                if(att != null && att.Url != null)
                {
                    opciones.Add(new Imagen {
                        Url = att.Url,
                        Autor = msg.Author
                    });
                }
            }
            Random rnd = new Random();
            Imagen meme = opciones[rnd.Next(opciones.Count)];

            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = new DiscordColor(78, 63, 96),
                Title = "Out of Context",
                Description = $"Imagen posteada por {meme.Autor.Username + "#" + meme.Autor.Discriminator}",
                ImageUrl = meme.Url
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
        }
    }
}
