using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class InteractuarSlashCommands : SlashCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [SlashCommand("Say", "Replica un texto")]
        public async Task Say(InteractionContext ctx, [Option("Texto", "El texto que quieres replicar")] string texto)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.DeleteResponseAsync();
            await ctx.Channel.SendMessageAsync(texto);
        }

        [SlashCommand("pregunta", "Responde con si o no")]
        public async Task Pregunta(InteractionContext ctx, [Option("Texto", "La pregunta que le quieres hacer")] string texto, [Option("Secreto", "Si quieres ver solo tu el comando")] bool secreto = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            { IsEphemeral = secreto }.AddEmbed(funciones.Pregunta(texto)));
        }

        [SlashCommand("elegir", "Elije entre varias opciones separadas por comas")]
        public async Task Elegir(InteractionContext ctx, [Option("Pregunta", "La pregunta que le quieres hacer")] string pregunta, [Option("Opciones", "Opciones separadas por comas")] string opc)
        {
            List<string> opciones = opc.Split(',').ToList();
            Random rnd = new();
            int random = rnd.Next(opciones.Count);
            string options = "**Opciones:**";
            foreach (string msj in opciones)
            {
                options += "\n   - " + msj;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = funciones.GetColor(),
                Title = "Pregunta",
                Description = "** " + pregunta + "**\n\n" + options + "\n\n**Respuesta:** " + opciones[random]
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        /* Comentado hasta que los SlashCommands soporten DiscordEmoji - Lo mismo para Stickers
        [SlashCommand("emote", "Replica un texto")]
        public async Task Emote(InteractionContext ctx, [Option("Emote", "El texto que quieres replicar")] string emote)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            //await ctx.DeleteResponseAsync();
            DiscordEmoji emoji;

            bool unicode = DiscordEmoji.TryFromUnicode(emote, out emoji);
            if (unicode)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(emoji));
            }
            else
            {
                emoji= DiscordEmoji.FromName(ctx.Client, ":WeSmart:");
                // Solo puede parsear las que el bot vea, no como el otro comando
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(emoji));
            }
        }*/

        [SlashCommand("avatar", "Obtiene un avatar")]
        public async Task Avatar(InteractionContext ctx, [Option("Usuario", "El usuario del avatar")] DiscordUser usuario = null, [Option("Secreto", "Si quieres ver solo tu el comando")] bool secreto = true)
        {
            usuario ??= ctx.Member;
            DiscordMember member = (DiscordMember)usuario;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Avatar de {member.DisplayName}",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx)
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = secreto
            }.AddEmbed(embed));
        }

        [SlashCommand("waifu", "Te dice mi nivel de waifu hacia un usuario")]
        public async Task Waifu(InteractionContext ctx, [Option("Usuario", "Usuario del que quieres saber el nivel de waifu")] DiscordUser usuario = null)
        {
            usuario ??= ctx.Member;
            DiscordMember miembro = (DiscordMember)usuario;
            string nombre;
            nombre = miembro.DisplayName;

            DiscordEmbedBuilder builder;
            int waifuLevel = funciones.GetNumeroRandom(0, 100);
            if (waifuLevel < 25)
            {
                builder = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe pego un tiro antes de tocarte.",
                    ImageUrl = "https://i.imgur.com/BOxbruw.png"
                };
            }
            else if (waifuLevel >= 25 && waifuLevel < 50)
            {
                builder = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Orange,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe das asquito, mejor me alejo de vos.",
                    ImageUrl = "https://i.imgur.com/ys2HoiL.jpg"
                };
            }
            else if (waifuLevel >= 50 && waifuLevel < 75)
            {
                builder = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Yellow,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nNo estás mal, quizas tengas posibilidades conmigo.",
                    ImageUrl = "https://i.imgur.com/h7Ic2rk.jpg"
                };
            }
            else if (waifuLevel >= 75 && waifuLevel < 100)
            {
                builder = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Green,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nSoy tu waifu, podes hacer lo que quieras conmigo.",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png"
                };
            }
            else // 100
            {
                builder = new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Blue,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\n.Estoy completamente enamorada de ti, ¿cuándo nos casamos?",
                    ImageUrl = "https://i.imgur.com/Vk6JMJi.jpg"
                };
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(builder));
        }

        [SlashCommand("love", "Muestra el porcentaje de amor entre dos usuarios")]
        public async Task Love(InteractionContext ctx, [Option("Usuario1", "Primer usuario")] DiscordUser user1 = null, [Option("Usuario2", "Segundo usuario")] DiscordUser user2 = null)
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = descripcion,
                ImageUrl = imagenUrl,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            }));
        }
    }
}
