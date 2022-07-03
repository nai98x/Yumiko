namespace Yumiko.Services
{
    public static class TriviaService
    {
        public static async Task HandleTriviaInteraction(ComponentInteractionCreateEventArgs e)
        {
            var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Guild.Id, e.Channel.Id);
            if (trivia != null)
            {
                var btnInteraction = e.Interaction;
                string modalId = $"quiz-modal-{btnInteraction.Id}";

                var modal = new DiscordInteractionResponseBuilder()
                    .WithCustomId(modalId)
                    .WithTitle($"{translations.guess_the} {trivia.Title}")
                    .AddComponents(new TextInputComponent(label: trivia.Title?.UppercaseFirst(), customId: "guess"));

                await btnInteraction.CreateResponseAsync(InteractionResponseType.Modal, modal);
            }
        }

        public static async Task HandleTriviaCancelledInteraction(ComponentInteractionCreateEventArgs e)
        {
            var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Guild.Id, e.Channel.Id);
            if (trivia != null)
            {
                if (trivia.CreatedBy?.Id == e.User.Id)
                {
                    trivia.Canceled = true;

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.you_have_cancelled_the_game,
                            Color = DiscordColor.Red,
                        }));
                }
            }
        }

        public static async Task HandleTriviaValueSubmittedInteraction(ModalSubmitEventArgs e)
        {
            var modalInteraction = e.Interaction;
            var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Interaction.Guild.Id, e.Interaction.Channel.Id);
            if (trivia != null)
            {
                var value = e.Values["guess"];
                if (trivia.CurrentRound.Matches.Contains(value.ToLower()))
                {
                    await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.you_guessed,
                            Color = DiscordColor.Green,
                        }));

                    trivia.CurrentRound.Guessed = true;
                    trivia.CurrentRound.Guesser = e.Interaction.User;
                    trivia.CurrentRound.GuessTime = modalInteraction.CreationTimestamp;
                }
                else
                {
                    await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.wrong_choice,
                            Description = $"{translations.your_attempt}: `{value}`",
                            Color = DiscordColor.Red,
                        }));
                }
            }
            else
            {
                await modalInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.error,
                            Description = translations.no_current_trivia,
                            Color = DiscordColor.Red,
                        }));
            }
        }
    }
}
