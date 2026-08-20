using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Games;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;

namespace Yumiko.Bot.Events.Handlers;

/// <summary>
/// Routes the component interactions Interactivity does not handle: the poll votes and the
/// trivia buttons. The rest are acknowledged so Discord does not show "the interaction failed".
/// </summary>
public sealed class ComponentInteractionHandler(
    PollState pollState,
    TriviaState triviaState,
    ILocalizer localizer,
    ILogger<ComponentInteractionHandler> logger)
{
    public const string PollSelectPrefix = "poll-select-";

    public async Task Handle(DiscordClient client, ComponentInteractionCreatedEventArgs args)
    {
        try
        {
            if (args.Id.StartsWith(PollSelectPrefix, StringComparison.Ordinal))
            {
                await HandlePollVoteAsync(args);
                return;
            }

            if (args.Id.StartsWith(TriviaCustomIds.CancelPrefix, StringComparison.Ordinal))
            {
                await HandleTriviaCancelAsync(args);
                return;
            }

            if (args.Id.StartsWith(TriviaCustomIds.RoundPrefix, StringComparison.Ordinal))
            {
                await HandleRoundAnswerAsync(args);
                return;
            }

            // Interactivity already takes care of the components a command is waiting for; here it is only
            // about avoiding Discord marking the interaction as failed.
            if (!args.Id.StartsWith("modal-", StringComparison.Ordinal) && !args.Id.StartsWith("cancel-", StringComparison.Ordinal))
            {
                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling the component interaction {Id}", args.Id);
        }
    }

    private async Task HandlePollVoteAsync(ComponentInteractionCreatedEventArgs args)
    {
        Loc loc = localizer.For(args.Interaction.Locale);

        string pollId = args.Id[PollSelectPrefix.Length..];
        Poll? poll = pollState.Get(pollId);
        string option = args.Values[0];

        if (poll is null || !poll.Vote(args.User.Id, option))
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            return;
        }

        await args.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.voted],
                Description = loc.Format(Keys.you_voted_to, option),
                Color = DiscordColor.Green,
            }).AsEphemeral());
    }

    /// <summary>Only whoever opened the match can cancel it.</summary>
    private async Task HandleTriviaCancelAsync(ComponentInteractionCreatedEventArgs args)
    {
        if (args.Guild is null || triviaState.Get(args.Guild.Id, args.Channel.Id) is not { } trivia)
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            return;
        }

        if (trivia.CreatedBy?.Id != args.User.Id)
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            return;
        }

        trivia.Canceled = true;
        Loc loc = localizer.For(args.Interaction.Locale);

        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
            .AsEphemeral()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.you_have_cancelled_the_game],
                Color = DiscordColor.Red,
            }));
    }

    private async Task HandleRoundAnswerAsync(ComponentInteractionCreatedEventArgs args)
    {
        Loc loc = localizer.For(args.Interaction.Locale);
        Trivia? trivia = args.Guild is null ? null : triviaState.Get(args.Guild.Id, args.Channel.Id);

        if (trivia is null)
        {
            await args.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.error],
                    Description = loc[Keys.no_current_trivia],
                    Color = DiscordColor.Red,
                }));
            return;
        }

        string attempt = args.Id[TriviaCustomIds.RoundPrefix.Length..];

        if (trivia.CurrentRound.Match != attempt)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = loc[Keys.wrong_choice],
                Color = DiscordColor.Red,
            };

            if (trivia.OptionNames.TryGetValue(attempt, out string? name))
            {
                embed.Description = $"{loc[Keys.your_attempt]}: `{name}`";
            }

            await args.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(embed));
            return;
        }

        trivia.CurrentRound.Guessed = true;
        trivia.CurrentRound.Guesser = args.User;
        trivia.CurrentRound.GuessTime = args.Interaction.CreationTimestamp;

        await args.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.you_guessed],
                Color = DiscordColor.Green,
            }));
    }
}
