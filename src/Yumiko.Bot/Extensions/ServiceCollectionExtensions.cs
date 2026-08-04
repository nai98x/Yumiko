using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Events;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.State;

namespace Yumiko.Bot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredDiscordClient(
        this IServiceCollection services,
        string discordToken,
        BotConfiguration botConfig,
        TimeoutSettings timeouts)
    {
        services.AddDiscordEventHandlers();

        // No fijar ShardingOptions.ShardCount: con el valor por defecto (null) el orquestador usa el
        // que recomienda Discord en /gateway/bot, que crece con la cantidad de guilds. Un número fijo
        // queda corto apenas el bot crece y Discord empieza a rechazar el IDENTIFY.
        services.AddShardedDiscordClient(discordToken, DiscordIntents.Guilds);

        services.AddInteractivityExtension(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromSeconds(timeouts.General),
            ButtonBehavior = ButtonPaginationBehavior.DeleteMessage,
            PaginationBehaviour = PaginationBehaviour.Ignore,
        });

        services.ConfigureEventHandlers(events => events.BindEventHandlers());

        services.AddCommandsExtension((provider, extension) =>
        {
            extension.AddDiscoveredSlashCommands(botConfig.LogGuildId);
            extension.AddDiscoveredContextMenuCommands(botConfig.LogGuildId);

            extension.AddProcessor(new SlashCommandProcessor(new SlashCommandConfiguration()));

            DiscordLogService logService = provider.GetRequiredService<DiscordLogService>();
            CommandUsageState usage = provider.GetRequiredService<CommandUsageState>();

            extension.CommandExecuted += (_, args) =>
            {
                logService.LogCommandExecuted(args.Context);
                usage.Increment($"/{args.Context.Command.FullName}");
                return Task.CompletedTask;
            };

            CommandErrorHandler errorHandler = new(
                logService,
                provider.GetRequiredService<ILocalizer>(),
                provider.GetRequiredService<TriviaState>());

            extension.CommandErrored += errorHandler.HandleAsync;
        }, new CommandsConfiguration
        {
            RegisterDefaultCommandProcessors = true,
            UseDefaultCommandErrorHandler = false,
        });

        return services;
    }
}
