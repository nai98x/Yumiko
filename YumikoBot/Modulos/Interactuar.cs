using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Interactuar : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("say"), Aliases("s"), Description("Yumiko habla en el chat.")]
        public async Task Say(CommandContext ctx, [Description("Mensaje para replicar")][RemainingText] string mensaje = null)
        {
            if (String.IsNullOrEmpty(mensaje))
            {
                var interactivity = ctx.Client.GetInteractivity();
                var msgAnime = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Escribe un mensaje",
                    Description = "Ejemplo: Hola! Soy Yumiko",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                });
                var msgAnimeInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
                if (!msgAnimeInter.TimedOut)
                {
                    mensaje = msgAnimeInter.Result.Content;
                    if (msgAnime != null)
                        await funciones.BorrarMensaje(ctx, msgAnime.Id);
                    if (msgAnimeInter.Result != null)
                        await funciones.BorrarMensaje(ctx, msgAnimeInter.Result.Id);
                }
                else
                {
                    var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = "Tiempo agotado esperando un mensaje",
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Red,
                    });
                    await Task.Delay(3000);
                    if (msgError != null)
                        await funciones.BorrarMensaje(ctx, msgError.Id);
                    if (msgAnime != null)
                        await funciones.BorrarMensaje(ctx, msgAnime.Id);
                    return;
                }
            }
            await ctx.Channel.SendMessageAsync(mensaje);
        }

        [Command("sayO"), Description("Yumiko habla en el chat."), Hidden, RequireOwner]
        public async Task SayO(CommandContext ctx, ulong guildId, ulong channelId, [Description("Mensaje para replicar")][RemainingText] string mensaje)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);
            if (guild != null)
            {
                var channel = guild.GetChannel(channelId);
                if (channel != null && funciones.ChequearPermisoYumiko(ctx, DSharpPlus.Permissions.SendMessages))
                {
                    await channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Color = funciones.GetColor(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            IconUrl = ctx.User.AvatarUrl,
                            Text = $"Enviado por {ctx.User.Username}"
                        },
                        Title = "Mensaje del administrador",
                        Description = mensaje
                    }).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Canal no encontrado");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Servidor no encontrado");
            }
        }

        [Command("pregunta"), Aliases("p", "question", "siono"), Description("Responde con SI O NO.")]
        public async Task Sisonon(CommandContext ctx, [Description("La pregunta que le quieres hacer")][RemainingText] string pregunta)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Red,
                        Title = "¿SI O NO?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NO"
                    }).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Green,
                        Title = "¿SI O NO?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SI"
                    }).ConfigureAwait(false);
                    break;
            }
        }

        [Command("elegir"), Aliases("e"), Description("Elige entre varias opciones.")]
        public async Task Elegir(CommandContext ctx, [Description("La pregunta de la cuál se eligirá una respuesta")][RemainingText] string pregunta)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordMessage mensajeBot = await ctx.Channel.SendMessageAsync("Ingrese las opciones separadas por comas").ConfigureAwait(false);
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author == ctx.User, TimeSpan.FromSeconds(60));
            if (!msg.TimedOut)
            {
                List<string> opciones = new List<string>();
                string msgResponse = msg.Result.Content;
                opciones = msgResponse.Split(',').ToList();
                Random rnd = new Random();
                int random = rnd.Next(opciones.Count);
                string options = "**Opciones:**";
                foreach (string msj in opciones)
                {
                    options += "\n   - " + msj;
                }
                await funciones.BorrarMensaje(ctx, mensajeBot.Id);
                await funciones.BorrarMensaje(ctx, msg.Result.Id);
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Title = "Pregunta",
                    Description = "** " + pregunta + "**\n\n" + options + "\n\n**Respuesta:** " + opciones[random]
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("No escribiste las opciones onii-chan" + ctx.User.Mention);
            }
        }

        [Command("emote"), Aliases("emoji"), Description("Muestra un emote en grande."), RequireGuild]
        public async Task Emote(CommandContext ctx, [Description("El emote para agrandar")] DiscordEmoji emote = null)
        {
            if (emote != null)
            {
                if (emote.Id != 0)
                {
                    string animado = emote.IsAnimated switch
                    {
                        true => "Si",
                        false => "No",
                    };
                    string dia = emote.CreationTimestamp.ToString("dddd", CultureInfo.CreateSpecificCulture("es"));
                    string mes = emote.CreationTimestamp.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Title = "Emote",
                        ImageUrl = emote.Url
                    }.AddField("Nombre", $"{emote.Name}", true)
                    .AddField("Identificador", $"{emote.Id}", true)
                    .AddField("Animado", $"{animado}", true)
                    .AddField("Fecha de creación", $"{funciones.UppercaseFirst(dia)} {emote.CreationTimestamp.Day} de {mes} del {emote.CreationTimestamp.Year}")
                    );
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(emote).ConfigureAwait(false);
                }
            }
            else
                {
                DiscordMessage msgError = await ctx.Channel.SendMessageAsync("Debes pasar un emote").ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msgError.Id);
            }
}

[Command("avatar"), Description("Muestra el avatar de un usuario."), RequireGuild]
        public async Task Avatar(CommandContext ctx, DiscordMember usuario = null)
        {
            if (usuario == null)
            {
                usuario = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"Avatar de {usuario.Username}#{usuario.Discriminator}",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
                Url = usuario.AvatarUrl
            }).ConfigureAwait(false);
        }

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
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Blue,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\n.Estoy completamente enamorada de ti, ¿cuándo nos casamos?",
                    ImageUrl = "https://i.imgur.com/Vk6JMJi.jpg"
                }).ConfigureAwait(false);
            }
        }

        [Command("love"), Description("Muestra el porcentaje de amor entre dos usuarios."), RequireGuild]
        public async Task Love(CommandContext ctx, DiscordMember user1 = null, DiscordMember user2 = null)
        {
            int porcentajeAmor;
            string titulo;
            if (user1 == null)
            {
                user1 = ctx.Member;
            }
            if ((user2 == null && user1.Id == ctx.Member.Id) || (user2 != null && user1.Id == user2.Id))
            {
                titulo = $"Amor propio de **{user1.Username}#{user1.Discriminator}**";
            }
            else
            {
                if (user2 == null)
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
            for (int i = 0; i < 20 - (porcentajeAmor / 5); i++)
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
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = descripcion,
                ImageUrl = imagenUrl,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
        }

        [Command("bestgirl"), Description("Best girl de Konosuba."), Cooldown(1, 300, CooldownBucketType.Guild)]
        public async Task BestGirl(CommandContext ctx)
        {
            Random rnd = new Random();
            int random = rnd.Next(3);
            switch (random)
            {
                case 0:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Cyan,
                        Title = "Best Girl (Konosuba)",
                        Description = "Aqua",
                        ImageUrl = "https://media.discordapp.net/attachments/816379048477065217/848254027639816202/b89362-ibkc0eoECaW1.png"
                    }).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Yellow,
                        Title = "Best Girl (Konosuba)",
                        Description = "Darkness",
                        ImageUrl = "https://media.discordapp.net/attachments/816379048477065217/848254149966561300/b89363-mm21Ll4NegUD.png"
                    }).ConfigureAwait(false);
                    break;
                case 2:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Red,
                        Title = "Best Girl (Konosuba)",
                        Description = "Megumin",
                        ImageUrl = "https://media.discordapp.net/attachments/816379048477065217/848254129084432428/b89361-x71P6YLrndd8.png"
                    }).ConfigureAwait(false);
                    break;
            }
        }

        [Command("ooc"), RequireNsfw]
        public async Task OOC(CommandContext ctx)
        {
            if (ctx.Guild.Id == 713809173573271613 || ctx.Guild.Id == 701813281718927441)
            {
                DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(787033852258418768);
                if (discordOOC == null)
                {
                    await ctx.RespondAsync("Error al obtener servidor **AniList ESP OOC**").ConfigureAwait(false);
                    return;
                }
                DiscordChannel channel = discordOOC.GetChannel(787033979274264577);
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
                    Color = new DiscordColor(78, 63, 96),
                    Title = "Out of Context",
                    ImageUrl = $"{meme.Url}"
                }).ConfigureAwait(false);
                await ctx.Message.DeleteAsync();
            }
            else
            {
                var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Comando no habilitado",
                    Description = $"Este comando no está disponible para este servidor",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }
    }
}
