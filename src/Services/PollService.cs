namespace Yumiko.Services
{
    public static class PollService
    {
        public static async Task HandleInteraction(ComponentInteractionCreateEventArgs e)
        {
            string pollId = e.Id["poll-select-".Length..];
            Poll? poll = Singleton.GetInstance().GetCurrentPoll(pollId);
            string selectedOption = e.Interaction.Data.Values[0];
            bool voted = false;
            if (poll != null)
            {
                var optionModel = poll.Options.Find(x => x.Name == selectedOption);
                if (optionModel != null)
                {
                    if (!optionModel.Voters.Any(x => x == e.User.Id))
                    {
                        optionModel.Voters.Add(e.User.Id);

                        poll.Options.ForEach(option =>
                        {
                            if (option.Name != selectedOption)
                            {
                                var oldVotes = option.Voters.Where(x => x.Equals(e.User.Id)).ToList();
                                oldVotes.ForEach(oldVote =>
                                {
                                    optionModel.Voters.Remove(oldVote);
                                });
                            }
                        });

                        voted = true;
                    }
                }
            }

            if (voted)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.voted,
                    Description = string.Format(translations.you_voted_to, selectedOption),
                    Color = DiscordColor.Green
                }).AsEphemeral(true));
            }
            else
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            }
        }
    }
}
