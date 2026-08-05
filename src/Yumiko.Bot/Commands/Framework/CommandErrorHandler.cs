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

        // A check that rejects the invocation is user input; one that throws is a bug, and its
        // message is an internal detail that neither goes to the user nor skips the log.
        bool rejectedByCheck = IsCheckRejection(args.Exception);

        DiscordEmbedBuilder embed = args.Exception switch
        {
            ChecksFailedException checks when rejectedByCheck =>
                ErrorEmbed.Forbidden(loc, ChecksMessage(checks.Errors.Select(e => e.ErrorMessage))),
            ParameterChecksFailedException checks when rejectedByCheck =>
                ErrorEmbed.Forbidden(loc, ChecksMessage(checks.Errors.Select(e => e.ErrorMessage))),
            AnilistRateLimitException => ErrorEmbed.Create(loc[Keys.error], loc[Keys.unknown_error]),
            AnilistServerErrorException => ErrorEmbed.Create(loc[Keys.error], loc[Keys.unknown_error]),
            _ => ErrorEmbed.Unknown(loc),
        };

        if (!rejectedByCheck)
        {
            await logService.LogExceptionAsync(args.Context.Guild, args.Context.Channel, args.Exception, $"Command /{args.Context.Command.FullName}");

            // If a trivia blew up the channel slot has to be released, otherwise it stays stuck forever.
            if (args.Context.Guild is not null && args.Exception.StackTrace?.Contains("Trivia", StringComparison.Ordinal) == true)
            {
                triviaState.Remove(args.Context.Guild.Id, args.Context.Channel.Id);
            }
        }

        await SendErrorAsync(args.Context, embed);
    }

    private static bool IsCheckRejection(Exception exception) => exception switch
    {
        ChecksFailedException => true,
        ParameterChecksFailedException checks => checks.Errors.All(e => e.Exception is null),
        _ => false,
    };

    private static string? ChecksMessage(IEnumerable<string?> errors)
    {
        string message = string.Join("\n", errors.Where(m => !string.IsNullOrWhiteSpace(m)));

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
            // The interaction may have expired (games last minutes); there is nothing else to do.
        }
    }
}
