using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class InteractuarSlashCommands : ApplicationCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [SlashCommand("Say", "Replicates the input")]
        public async Task Say(InteractionContext ctx, [Option("Text", "The input you want to replicate ")] string texto)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.DeleteResponseAsync();
            await ctx.Channel.SendMessageAsync(texto);
        }

        [SlashCommand("question", "Answer with yes or no")]
        public async Task Pregunta(InteractionContext ctx, [Option("Question", "The question you want to ask")] string texto)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(funciones.Pregunta(texto)));
        }

        [SlashCommand("choose", "Choose from several options separated by commas")]
        public async Task Elegir(InteractionContext ctx, [Option("Question", "The question you want to ask")] string pregunta, [Option("Options", "Options separated by commas")] string opc)
        {
            List<string> opciones = opc.Split(',').ToList();
            Random rnd = new();
            int random = rnd.Next(opciones.Count);
            string options = "**Options:**";
            foreach (string msj in opciones)
            {
                options += "\n   - " + msj;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = funciones.GetColor(),
                Title = "Question",
                Description = "** " + pregunta + "**\n\n" + options + "\n\n**Answer:** " + opciones[random]
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

        [SlashCommand("avatar", "Get an avatar")]
        public async Task Avatar(InteractionContext ctx, [Option("Usuario", "El usuario del avatar")] DiscordUser usuario = null, [Option("Secreto", "Si quieres ver solo tu el comando")] bool secreto = true)
        {
            usuario ??= ctx.Member;
            DiscordMember member = (DiscordMember)usuario;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName}'s avatar",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx)
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = secreto
            }.AddEmbed(embed));
        }

        [SlashCommand("waifu", "My waifu level towards a user ")]
        public async Task Waifu(InteractionContext ctx, [Option("User", "User whose waifu level you want to know")] DiscordUser usuario = null)
        {
            usuario ??= ctx.Member;
            DiscordMember miembro = (DiscordMember)usuario;
            var builder = funciones.Waifu(miembro);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(builder));
        }

        [SlashCommand("love", "Shows the percentage of love between two users")]
        public async Task Love(InteractionContext ctx, [Option("User1", "First user")] DiscordUser user1 = null, [Option("User2", "Second user")] DiscordUser user2 = null)
        {
            int porcentajeAmor;
            string titulo;
            if (user1 == null)
            {
                user1 = ctx.Member;
            }
            if ((user2 == null && user1.Id == ctx.Member.Id) || (user2 != null && user1.Id == user2.Id))
            {
                titulo = $"Self love of **{user1.Username}#{user1.Discriminator}**";
            }
            else
            {
                if (user2 == null)
                {
                    user2 = user1;
                    user1 = ctx.Member;
                }
                titulo = $"Love between **{user1.Username}#{user1.Discriminator}** and **{user2.Username}#{user2.Discriminator}**";
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
                    descripcion += "Get away now! You two are going to kill each other.\n";
                else if (porcentajeAmor > 0 && porcentajeAmor <= 10)
                    descripcion += "You will hate each other, you two are not compatible at all.\n";
                else if (porcentajeAmor > 10 && porcentajeAmor <= 25)
                    descripcion += "The best thing is that they move away from each other, you do not fit.\n";
                else if (porcentajeAmor > 25 && porcentajeAmor <= 50)
                    descripcion += "You will be good friends, but I see love difficult.\n";
                else if (porcentajeAmor > 50 && porcentajeAmor <= 75)
                    descripcion += "You are most likely best friends and hopefully something more.\n";
                else if (porcentajeAmor > 75 && porcentajeAmor <= 90)
                    descripcion += "You guys have a lot of chemistry, you have to give a chance.\n";
                else if (porcentajeAmor > 90 && porcentajeAmor <= 99)
                    descripcion += "You two are meant to be together.\n";
                else
                    descripcion += "Perfect relationship! You will marry and have many children.\n";
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
