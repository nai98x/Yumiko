using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class SlashCommands : SlashCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

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
    }
}
