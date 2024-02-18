namespace Yumiko.Services
{
    public static class TriviaService
    {
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

        public static async Task HandleTriviaRoundInteraction(ComponentInteractionCreateEventArgs e)
        {
            var trivia = Singleton.GetInstance().GetCurrentTrivia(e.Guild.Id, e.Channel.Id);
            if (trivia != null)
            {
                string guess = e.Id[11..];

                if (trivia.CurrentRound.Match == guess)
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.you_guessed,
                            Color = DiscordColor.Green,
                        }));

                    trivia.CurrentRound.Guessed = true;
                    trivia.CurrentRound.Guesser = e.User;
                    trivia.CurrentRound.GuessTime = e.Interaction.CreationTimestamp;
                }
                else
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.wrong_choice,
                            Description = $"{translations.your_attempt}: `{guess}`",
                            Color = DiscordColor.Red,
                        }));
                }
            }
            else
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
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
