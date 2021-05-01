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
            shipeoUsr = ctxMiembro.Mention;
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 818293607709933602);
            var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Shippeo",
                Description = $"Shippeo a {shipeoUsr} con {elegido.Mention} 💘",
                ImageUrl = elegida.Url
            }).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":question:")).ConfigureAwait(false);
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
            Imagen elegida = await funciones.GetImagenDiscordYumiko(ctx, 818293607709933602);
            var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Title = "Shippeo random",
                Description = $"Shippeo a {elegido.Mention} con {elegido2.Mention} 💘",
                ImageUrl = elegida.Url
            }).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")).ConfigureAwait(false);
            await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":question:")).ConfigureAwait(false);
        }

        [Command("love"), Description("Muestra el porcentaje de amor entre dos usuarios."), RequireGuild]
        public async Task Love(CommandContext ctx, DiscordMember user1, DiscordMember user2 = null)
        {
            int porcentajeAmor;
            string titulo;
            if((user2 == null && user1.Id == ctx.Member.Id) || (user2 != null && user1.Id == user2.Id))
            {
                titulo = $"Amor propio de **{user1.Username}#{user1.Discriminator}**";
            }
            else
            {
                if(user2 == null)
                {
                    user2 = user1;
                    user1 = ctx.Member;
                }
                titulo = $"Amor entre **{user1.Username}#{user1.Discriminator}** y **{user2.Username}#{user2.Discriminator}**";
            }
            porcentajeAmor = funciones.GetNumeroRandom(0, 100);
            string descripcion = $"**{porcentajeAmor}%** [";
            string imagenUrl;
            for (int i = 0; i < porcentajeAmor / 5; i++)
            {
                descripcion += "█";
            }
            for (int i=0; i<20-(porcentajeAmor/5); i++)
            {
                descripcion += " . ";
            }
            descripcion += "]\n\n";
            if ((user2 == null && user1.Id != ctx.Member.Id) || (user2 != null && user1.Id != user2.Id))
            {
                if (porcentajeAmor == 0)
                    descripcion += "¡Aléjense ya! Ustedes dos se van a matar.\n";
                else if (porcentajeAmor > 0 && porcentajeAmor <= 10)
                    descripcion += "Se odiarán mutuamente, no son para nada compatibles.\n";
                else if (porcentajeAmor > 10 && porcentajeAmor <= 25)
                    descripcion += "Lo mejor es que se alejen uno del otro, no encajan.\n";
                else if (porcentajeAmor > 25 && porcentajeAmor <= 50)
                    descripcion += "Serán buenos amigos, pero veo dificil el amor.\n";
                else if (porcentajeAmor > 50 && porcentajeAmor <= 75)
                    descripcion += "Lo más probable es que sean mejores amigos y con suerte algo más.\n";
                else if (porcentajeAmor > 75 && porcentajeAmor <= 90)
                    descripcion += "Tienen mucha química, tienen que darse una oportunidad.\n";
                else if (porcentajeAmor > 90 && porcentajeAmor <= 99)
                    descripcion += "Ustedes dos están destinados a estar juntos.\n";
                else
                    descripcion += "¡Relación perfecta! Se casarán y tendran muchos hijos.\n";
            }
            if (porcentajeAmor <= 25)
                imagenUrl = "https://i.imgur.com/BOxbruw.png";
            else if (porcentajeAmor > 25 && porcentajeAmor <= 50)
                imagenUrl = "https://i.imgur.com/ys2HoiL.jpg";
            else if (porcentajeAmor > 50 && porcentajeAmor <= 75)
                imagenUrl = "https://i.imgur.com/h7Ic2rk.jpg";
            else if (porcentajeAmor > 75 && porcentajeAmor <= 99)
                imagenUrl = "https://i.imgur.com/dhXR8mV.png";
            else
                imagenUrl = "https://i.imgur.com/Vk6JMJi.jpg";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder { 
                Title = titulo,
                Description = descripcion,
                ImageUrl = imagenUrl,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
        }
    }
}
