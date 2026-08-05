using DSharpPlus.Commands.Processors.SlashCommands;
using Yumiko.Bot.Games;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;

namespace Yumiko.Bot.Helpers;

/// <summary>Lets the user pick an AniList genre.</summary>
public sealed class GenreSelector(GamePool pool, DiscordInteractivity discordInteractivity)
{
    public async Task<string?> ChooseAsync(SlashCommandContext ctx, Loc loc)
    {
        List<string> genres = await pool.GetGenresAsync(ctx.Channel.IsNSFW);

        if (genres.Count == 0)
        {
            return null;
        }

        int? chosen = await discordInteractivity.ChooseAsync(
            ctx,
            [.. genres.Select(g => new TitleDescription { Title = g })],
            loc);

        return chosen is null ? null : genres[chosen.Value];
    }
}
