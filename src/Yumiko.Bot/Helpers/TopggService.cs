using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// Wraps <c>ITopggClient</c> with the "only if it is enabled and we are not in debug" gating.
/// </summary>
public sealed class TopggService(
    ITopggClient topgg,
    TopggSettings settings,
    DiscordBotService discordBotService,
    ILogger<TopggService> logger)
{
    public bool Enabled => settings.Enabled && !discordBotService.Debug;

    public async Task UpdateStatsAsync(DiscordClient client)
    {
        if (!Enabled)
        {
            return;
        }

        try
        {
            await topgg.UpdateStatsAsync(client.CurrentApplication.Id, client.Guilds.Count, 1);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not update the stats on top.gg");
        }
    }

    public async Task<int> GetMonthlyVotesAsync(DiscordClient client)
    {
        if (!Enabled)
        {
            throw new NotSupportedException("top.gg is not enabled.");
        }

        return await topgg.GetMonthlyVotesCountAsync(client.CurrentApplication.Id);
    }

    /// <summary>
    /// If the user has not voted yet, returns the embed to remind them. <c>null</c> if they already voted,
    /// if top.gg is disabled, or if the query failed (the experience is not cut short because of that).
    /// </summary>
    public async Task<DiscordEmbedBuilder?> GetVoteReminderAsync(DiscordClient client, ulong userId, Loc loc)
    {
        if (!Enabled)
        {
            return null;
        }

        try
        {
            if (await topgg.HasVotedAsync(client.CurrentApplication.Id, userId))
            {
                return null;
            }

            string url = $"https://top.gg/bot/{client.CurrentUser.Id}/vote";

            return new DiscordEmbedBuilder
            {
                Title = loc[Keys.vote_me_on_topgg],
                Description = loc.Format(Keys.vote_me_on_topgg_desc, url),
                Color = YumikoColors.Primary,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = loc[Keys.message_will_not_be_triggered] },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not query the vote on top.gg");
            return null;
        }
    }
}
