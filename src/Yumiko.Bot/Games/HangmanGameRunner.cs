using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Games;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;

namespace Yumiko.Bot.Games;

/// <summary>Data of the word to guess, already resolved to text by the command.</summary>
public sealed record HangmanTarget(string Word, string? Image, string Reveal, string Mode);

public sealed class HangmanGameRunner(InteractivityExtension interactivity)
{
    private static readonly TimeSpan ButtonTimeout = TimeSpan.FromSeconds(30);

    public async Task PlayAsync(SlashCommandContext ctx, HangmanTarget target, Loc loc)
    {
        HangmanState state = new(target.Word);
        DiscordUser? winner = null;

        DiscordEmbedBuilder embed = new()
        {
            Title = $"{loc[Keys.hangman]} ({target.Mode})",
            Description = loc[Keys.type_a_letter],
            Color = YumikoColors.Primary,
        };

        while (true)
        {
            Guid guid = Guid.NewGuid();

            DiscordMessage message = await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed)
                .AddActionRowComponent(
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, $"modal-letter-{guid}", loc[Keys.type_a_letter]),
                    new DiscordButtonComponent(DiscordButtonStyle.Success, $"modal-guess-{guid}", loc[Keys.guess]),
                    new DiscordButtonComponent(DiscordButtonStyle.Danger, $"cancel-{guid}", loc[Keys.finish_game])));

            InteractivityResult<ComponentInteractionCreatedEventArgs> button =
                await interactivity.WaitForButtonAsync(message, ButtonTimeout);

            if (button.TimedOut)
            {
                state.AddMistake();
                embed = Notice(loc[Keys.did_not_press_button_to_write], loc[Keys.write_any_letter_to_continue]);
            }
            else if (button.Result.Id.StartsWith("cancel-", StringComparison.Ordinal))
            {
                state.Surrender();
            }
            else
            {
                (DiscordEmbedBuilder newEmbed, DiscordUser? guesser) =
                    await HandleModalAsync(button.Result, message, state, target, embed, loc);

                embed = newEmbed;
                winner ??= guesser;
            }

            if (state.IsFinished || winner is not null)
            {
                break;
            }
        }

        await ShowEndAsync(ctx, state, target, winner, loc);
    }

    private async Task<(DiscordEmbedBuilder Embed, DiscordUser? Winner)> HandleModalAsync(
        ComponentInteractionCreatedEventArgs button,
        DiscordMessage message,
        HangmanState state,
        HangmanTarget target,
        DiscordEmbedBuilder currentEmbed,
        Loc loc)
    {
        DiscordInteraction buttonInteraction = button.Interaction;
        string modalId = $"modal-{buttonInteraction.Id}";

        await buttonInteraction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordModalBuilder()
            .WithCustomId(modalId)
            .WithTitle(loc[Keys.hangman])
            .AddTextInput(new DiscordTextInputComponent("value"), loc[Keys.guess]));

        InteractivityResult<ModalSubmittedEventArgs> modal =
            await interactivity.WaitForModalAsync(modalId, buttonInteraction.User);

        if (modal.TimedOut)
        {
            state.AddMistake();
            return (Notice(loc[Keys.did_not_write_on_time], loc[Keys.write_any_letter_to_continue]), null);
        }

        DiscordInteraction modalInteraction = modal.Result.Interaction;
        await modalInteraction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        string value = modal.Result.TextOf("value");
        TimeSpan elapsed = modalInteraction.CreationTimestamp - message.CreationTimestamp;

        if (button.Id.StartsWith("modal-guess-", StringComparison.Ordinal))
        {
            bool isCorrect = string.Equals(value.Trim(), target.Word.Trim(), StringComparison.OrdinalIgnoreCase);

            if (!isCorrect)
            {
                return (currentEmbed, null);
            }

            state.RevealAll();
            return (currentEmbed, modalInteraction.User);
        }

        bool letterWasCorrect = state.Guess(value);

        return (new DiscordEmbedBuilder
        {
            Title = letterWasCorrect
                ? loc.Format(Keys.user_has_guessed, DisplayNameOf(modalInteraction.User))
                : loc.Format(Keys.user_made_a_mistake, DisplayNameOf(modalInteraction.User)),
            Description = Board(state, loc),
            Color = letterWasCorrect ? DiscordColor.Green : DiscordColor.Red,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{loc[Keys.time]}: {elapsed.TotalSeconds:0.##}s",
                IconUrl = modalInteraction.User.AvatarUrl,
            },
        }, null);
    }

    private static async Task ShowEndAsync(
        SlashCommandContext ctx,
        HangmanState state,
        HangmanTarget target,
        DiscordUser? winner,
        Loc loc)
    {
        if (state.IsLost && winner is null)
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.defeat],
                Description = target.Reveal,
                ImageUrl = target.Image,
                Color = DiscordColor.Red,
            }));
            return;
        }

        string description = target.Reveal;

        if (winner is not null)
        {
            description += $"\n\n{loc[Keys.winner]}: {winner.Mention}";
        }

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.victory],
                Description = description,
                ImageUrl = target.Image,
                Color = DiscordColor.Green,
            })
            .AddMention(new UserMention()));
    }

    private static string Board(HangmanState state, Loc loc)
    {
        // The spaces close and reopen the inline block: otherwise Discord collapses them.
        string letters = "`" + string.Concat(state.Word.Select(c =>
            c == ' ' ? "` `" : state.IsRevealed(c) ? $"{c} " : "_ ")) + "`\n\n";

        return letters +
               HangmanArt.Draw(state.Mistakes) +
               $"\n{Formatter.Bold($"{loc[Keys.letters_used]}:")}\n" +
               string.Join(" ", state.UsedLetters.Select(Formatter.InlineCode));
    }

    private static DiscordEmbedBuilder Notice(string title, string description) => new()
    {
        Title = title,
        Description = description,
        Color = DiscordColor.Red,
    };

    private static string DisplayNameOf(DiscordUser user) =>
        user is DiscordMember member ? member.DisplayName : user.Username;
}
