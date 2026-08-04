using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;

namespace Yumiko.Bot.Extensions;

public static class CommandContextExtensions
{
    /// <summary>Localizador atado al idioma que el usuario tiene configurado en Discord.</summary>
    public static Loc Loc(this CommandContext ctx, ILocalizer localizer) =>
        localizer.For(ctx is SlashCommandContext slash ? slash.Interaction.Locale : null);

    /// <summary>
    /// Verifica que el bot haya terminado de inicializarse. Si no, responde avisando y devuelve
    /// <c>false</c> para que el comando corte antes de diferir.
    /// </summary>
    public static async Task<bool> EnsureBotReadyAsync(this CommandContext ctx, DiscordBotService discordBotService, Loc loc)
    {
        if (discordBotService.Initialized || discordBotService.Debug)
        {
            return true;
        }

        DiscordEmbedBuilder embed = new()
        {
            Title = loc[Keys.error],
            Description = loc[Keys.bot_not_ready],
            Color = DiscordColor.Red,
        };

        if (ctx is SlashCommandContext slashCtx)
        {
            await slashCtx.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
        }
        else
        {
            await ctx.RespondAsync(embed);
        }

        return false;
    }

    public static async Task DeferEphemeralAsync(this CommandContext ctx)
    {
        if (ctx is SlashCommandContext slashCtx)
        {
            await slashCtx.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral());
        }
        else
        {
            await ctx.DeferResponseAsync();
        }
    }
}
