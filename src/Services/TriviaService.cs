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
    }
}
