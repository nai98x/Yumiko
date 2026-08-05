using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace Yumiko.Bot.Services.Scheduling;

/// <summary>
/// Base of the scheduled tasks.
/// </summary>
/// <remarks>
/// The cron expressions are evaluated in <b>UTC</b>: Yumiko is a global multi-guild bot, so there is no
/// "server" time zone worth privileging.
/// </remarks>
public abstract class CronBackgroundService(DiscordBotService discordBotService, ILogger logger) : BackgroundService
{
    protected bool Initialized => discordBotService.Initialized;

    protected abstract string CronExpression { get; }

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (discordBotService.Debug)
        {
            logger.LogInformation("Scheduled tasks disabled in debug mode ({Task})", GetType().Name);
            return;
        }

        CrontabSchedule schedule = CrontabSchedule.Parse(CronExpression);
        TimeSpan maxDelay = TimeSpan.FromMilliseconds(int.MaxValue);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Task.Delay can wake up a few ms before the target; without this check the task would run
            // on the previous side of the edge (wrong hour/day) and again right away, duplicating the work.
            DateTime target = schedule.GetNextOccurrence(DateTime.UtcNow);

            TimeSpan remaining;
            while ((remaining = target - DateTime.UtcNow) > TimeSpan.Zero)
            {
                await Task.Delay(remaining > maxDelay ? maxDelay : remaining, stoppingToken);
            }

            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running the scheduled task {Task}", GetType().Name);
            }
        }
    }
}
