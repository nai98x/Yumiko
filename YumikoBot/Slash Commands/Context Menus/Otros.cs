using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class InteractuarConextMenus : ApplicationCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Avatar")]
        public async Task Avatar(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{ctx.TargetMember.DisplayName}'s avatar",
                ImageUrl = ctx.TargetMember.AvatarUrl,
                Color = funciones.GetColor()
            }).AsEphemeral(true));
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Waifu")]
        public async Task Waifu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(funciones.Waifu(ctx.TargetMember)).AsEphemeral(false));
        }
    }
}
