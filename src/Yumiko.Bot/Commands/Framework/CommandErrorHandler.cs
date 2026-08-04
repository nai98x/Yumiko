using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Exceptions;

namespace Yumiko.Bot.Commands.Framework;

public sealed class CommandErrorHandler(DiscordLogService logService, ILocalizer localizer, TriviaState triviaState)
{
    public async Task HandleAsync(CommandsExtension _, CommandErroredEventArgs args)
    {
        Loc loc = args.Context.Loc(localizer);

        DiscordEmbedBuilder embed = args.Exception switch
        {
            ChecksFailedException checks => ErrorEmbed.Forbidden(loc, ChecksMessage(checks)),
            AnilistRateLimitException => ErrorEmbed.Create(loc[Keys.error], loc[Keys.unknown_error]),
            AnilistServerErrorException => ErrorEmbed.Create(loc[Keys.error], loc[Keys.unknown_error]),
            _ => ErrorEmbed.Unknown(loc),
        };

        if (args.Exception is not ChecksFailedException)
        {
            await logService.LogExceptionAsync(args.Context.Guild, args.Context.Channel, args.Exception, $"Comando /{args.Context.Command.FullName}");

            // Si reventó una trivia hay que soltar el slot del canal, si no queda trabado para siempre.
            if (args.Context.Guild is not null && args.Exception.StackTrace?.Contains("Trivia", StringComparison.Ordinal) == true)
            {
                triviaState.Remove(args.Context.Guild.Id, args.Context.Channel.Id);
            }
        }

        await SendErrorAsync(args.Context, embed);
    }

    private static string? ChecksMessage(ChecksFailedException checks)
    {
        string message = string.Join("\n", checks.Errors
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m)));

        return string.IsNullOrWhiteSpace(message) ? null : message;
    }

    private static async ValueTask SendErrorAsync(CommandContext ctx, DiscordEmbedBuilder embed)
    {
        try
        {
            DiscordMessageBuilder message = new DiscordMessageBuilder().AddEmbed(embed);

            if (ctx is SlashCommandContext { Interaction.ResponseState: not DiscordInteractionResponseState.Unacknowledged })
            {
                await ctx.FollowupAsync(message);
            }
            else
            {
                await ctx.RespondAsync(message);
            }
        }
        catch
        {
            // La interacción pudo haber vencido (los juegos duran minutos); no hay nada más que hacer.
        }
    }
}
