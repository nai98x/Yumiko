using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// Envuelve a <c>ITopggClient</c> con el gating de "solo si está habilitado y no estamos en debug".
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
            logger.LogError(ex, "No se pudieron actualizar las stats en top.gg");
        }
    }

    public async Task<int> GetMonthlyVotesAsync(DiscordClient client)
    {
        if (!Enabled)
        {
            throw new NotSupportedException("top.gg no está habilitado.");
        }

        return await topgg.GetMonthlyVotesCountAsync(client.CurrentApplication.Id);
    }

    /// <summary>
    /// Si el usuario no votó todavía, devuelve el embed para recordárselo. <c>null</c> si ya votó,
    /// si top.gg está deshabilitado, o si la consulta falló (no se le corta la experiencia por eso).
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
            logger.LogError(ex, "No se pudo consultar el voto en top.gg");
            return null;
        }
    }
}
