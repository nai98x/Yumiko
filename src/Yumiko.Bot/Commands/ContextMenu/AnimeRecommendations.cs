using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Entities;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Commands.ContextMenu;

[TestCommand]
public sealed class AnimeRecommendations(ILocalizer localizer, AnilistResponses responses)
{
    [Command("Anime Recommendations")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu)]
    public async Task ExecuteAsync(UserCommandContext ctx, DiscordUser target)
    {
        Loc loc = ctx.Loc(localizer);

        // Asking for your own recommendations answers privately; over someone else, in plain sight.
        await ctx.DeferResponseAsync(ctx.User.Id == target.Id);
        await ctx.EditResponseAsync(await responses.RecommendationsAsync(target, MediaType.ANIME, loc));
    }
}
