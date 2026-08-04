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
public sealed class MangaRecommendations(ILocalizer localizer, AnilistResponses responses)
{
    [Command("Manga Recommendations")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    [SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu)]
    public async Task ExecuteAsync(UserCommandContext ctx, DiscordUser target)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync(ctx.User.Id == target.Id);
        await ctx.EditResponseAsync(await responses.RecommendationsAsync(target, MediaType.MANGA, loc));
    }
}
