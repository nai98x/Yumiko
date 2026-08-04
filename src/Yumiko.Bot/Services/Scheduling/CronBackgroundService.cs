using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace Yumiko.Bot.Services.Scheduling;

/// <summary>
/// Base de las tareas programadas.
/// </summary>
/// <remarks>
/// Las expresiones cron se evalúan en <b>UTC</b>: Yumiko es un bot multi-guild global, así que no hay
/// un huso horario "del servidor" que tenga sentido privilegiar.
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
            logger.LogInformation("Tareas programadas deshabilitadas en modo debug ({Task})", GetType().Name);
            return;
        }

        CrontabSchedule schedule = CrontabSchedule.Parse(CronExpression);
        TimeSpan maxDelay = TimeSpan.FromMilliseconds(int.MaxValue);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Task.Delay puede despertar unos ms antes del objetivo; sin este chequeo la tarea correría
            // del lado anterior del borde (hora/día equivocados) y otra vez al instante, duplicando el trabajo.
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
                logger.LogError(ex, "Error ejecutando la tarea programada {Task}", GetType().Name);
            }
        }
    }
}
