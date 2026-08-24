using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Yumiko.Application.Backups;
using Yumiko.Bot.Configuration;

namespace Yumiko.Bot.Services.Scheduling.Tasks;

/// <summary>
/// Warns in the config channel when a day went by without a backup of the database.
/// </summary>
/// <remarks>
/// It checks that the script finished well, not that the file is still at the provider: it covers
/// the real failures (timer down, expired credentials, broken dump) but not someone emptying the bucket.
/// </remarks>
public sealed class BackupScheduledService(
    DiscordBotService discordBotService,
    BackupsSettings settings,
    ILogger<BackupScheduledService> logger)
    : CronBackgroundService(discordBotService, logger)
{
    /// <summary>13:00 UTC: 10:00 in the server time zone, a couple of hours after the backup runs.</summary>
    protected override string CronExpression => "0 13 * * *";

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        if (!Initialized || settings.StatePath.Length == 0)
        {
            return;
        }

        string? mark = File.Exists(settings.StatePath)
            ? await File.ReadAllTextAsync(settings.StatePath, cancellationToken)
            : null;

        if (BackupState.IsUpToDate(mark, DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            return;
        }

        logger.LogWarning("No backup detected for today. Last successful backup: {Mark}", BackupState.DescribeLast(mark));

        await Bot.LogChannelConfigBots.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Backup missing",
            Description = $"No backup of the database was detected today.\nLast successful backup: **{BackupState.DescribeLast(mark)}**.",
            Color = DiscordColor.Orange,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"{DateTimeOffset.Now}" },
        });
    }
}
