using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Fun : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("waifu"), Description("Te dice mi nivel de waifu hacia un usuario.")]
        public async Task Waifu(CommandContext ctx, [Description("Miembro para ver su nivel de waifu, si se deja vacío lo hace de tí")] DiscordMember miembro = null)
        {
            string nombre;
            if (miembro == null)
                nombre = ctx.Member.DisplayName;
            else
                nombre = miembro.DisplayName;

            int waifuLevel = funciones.GetNumeroRandom(0, 100);
            if (waifuLevel < 25)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
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
                    Footer = funciones.GetFooter(ctx),
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
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Yellow,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nNo estás mal, quizas tengas posibilidades conmigo.",
                    ImageUrl = "https://i.imgur.com/h7Ic2rk.jpg"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 75 && waifuLevel < 100)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
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
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Blue,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\n.Estoy completamente enamorada de ti, ¿cuándo nos casamos?",
                    ImageUrl = "https://i.imgur.com/Vk6JMJi.jpg"
                }).ConfigureAwait(false);
            }
        }

        [Command("husbando"), Description("Elijo a mi husbando del servidor."), RequireGuild]
        public async Task Husbando(CommandContext ctx)
        {
            Random rnd = new Random();
            DiscordMember elegido;
            if (ctx.Guild.Id == 701813281718927441) // Anilist ESP
            {
                DiscordRole kohai = ctx.Guild.GetRole(713484136001700042);
                DiscordRole senpai = ctx.Guild.GetRole(713484281950765138);
                DiscordRole master = ctx.Guild.GetRole(713484545944584273);
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false && (x.Value.Roles.Contains(kohai) || x.Value.Roles.Contains(senpai) || x.Value.Roles.Contains(master)));
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            else
            {
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false);
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Husbando",
                Description = $"Mi husbando es: **{elegido.DisplayName} ({elegido.Username}#{elegido.Discriminator})** 💘",
                ImageUrl = elegido.AvatarUrl
            }).ConfigureAwait(false);
        }

        [Command("ship"), Description("Te digo con quien te shippearia."), RequireGuild]
        public async Task Ship(CommandContext ctx, [Description("Usuario para shippear, si se deja vacío lo hace de tí")] DiscordUser usuario = null)
        {
            if (usuario == null)
            {
                usuario = ctx.User;
            }
            Random rnd = new Random();
            DiscordMember elegido;
            if (ctx.Guild.Id == 701813281718927441) // Anilist ESP
            {
                DiscordRole kohai = ctx.Guild.GetRole(713484136001700042);
                DiscordRole senpai = ctx.Guild.GetRole(713484281950765138);
                DiscordRole master = ctx.Guild.GetRole(713484545944584273);
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false && (x.Value.Roles.Contains(kohai) || x.Value.Roles.Contains(senpai) || x.Value.Roles.Contains(master)) && x.Value.Id != usuario.Id);
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            else
            {
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false && x.Value.Id != usuario.Id);
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            string shipeoUsr;
            DiscordMember ctxMiembro = await ctx.Guild.GetMemberAsync(usuario.Id);
            shipeoUsr = ctxMiembro.DisplayName;
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Shippeo",
                Description = $"Shippeo a **{shipeoUsr}** con **{elegido.DisplayName}** 💘",
                ImageUrl = funciones.GetImagenRandomShip()
            }).ConfigureAwait(false);
        }

        [Command("shipr"), Description("Eligo un ship en el servidor."), RequireGuild]
        public async Task ShipRandom(CommandContext ctx)
        {
            Random rnd = new Random();
            DiscordMember elegido, elegido2;
            if (ctx.Guild.Id == 701813281718927441) // Anilist ESP
            {
                DiscordRole kohai = ctx.Guild.GetRole(713484136001700042);
                DiscordRole senpai = ctx.Guild.GetRole(713484281950765138);
                DiscordRole master = ctx.Guild.GetRole(713484545944584273);
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false && (x.Value.Roles.Contains(kohai) || x.Value.Roles.Contains(senpai) || x.Value.Roles.Contains(master)));
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
                elegido2 = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            else
            {
                var miembros = ctx.Guild.Members.Where(x => x.Value.IsBot == false);
                elegido = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
                elegido2 = miembros.ElementAt(funciones.GetNumeroRandom(0, miembros.Count() - 1)).Value;
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Shippeo random",
                Description = $"Shippeo a **{elegido.DisplayName}** con **{elegido2.DisplayName}** 💘",
                ImageUrl = funciones.GetImagenRandomShip()
            }).ConfigureAwait(false);
        }

        [Command("ooc"), Description("Imagen aleatoria de Out of Context."), RequireGuild, RequireNsfw]
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

                foreach (DiscordMessage mensaje in mensajesAux)
                {
                    msgs.Add(mensaje);
                }
            }
            List<Imagen> opciones = new List<Imagen>();
            foreach (DiscordMessage msg in msgs)
            {
                var att = msg.Attachments.FirstOrDefault();
                if (att != null && att.Url != null)
                {
                    opciones.Add(new Imagen
                    {
                        Url = att.Url,
                        Autor = msg.Author
                    });
                }
            }
            Random rnd = new Random();
            Imagen meme = opciones[rnd.Next(opciones.Count)];
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Out of Context",
                ImageUrl = meme.Url
            }).ConfigureAwait(false);
        }
    }
}
