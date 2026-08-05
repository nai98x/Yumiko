using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;

namespace Yumiko.Bot.Extensions;

public static class CommandContextExtensions
{
    /// <summary>Localizer bound to the language the user has configured in Discord.</summary>
    public static Loc Loc(this CommandContext ctx, ILocalizer localizer) =>
        localizer.For(ctx is SlashCommandContext slash ? slash.Interaction.Locale : null);

    /// <summary>
    /// Checks that the bot finished initializing. If not, it answers with a notice and returns
    /// <c>false</c> so the command stops before deferring.
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
