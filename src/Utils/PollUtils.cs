﻿namespace Yumiko.Utils
{
    public static class PollUtils
    {
        public static DiscordEmbedBuilder GetResultsEmbed(Poll poll, bool anonymous)
        {
            var embed = new DiscordEmbedBuilder();

            if (poll.Options.Select(x => x.VotesCount > 0).Any())
            {
                string votes = string.Empty;
                var options = poll.Options.OrderByDescending(x => x.VotesCount).ToList();
                if (anonymous)
                {
                    votes = string.Join("\n", options.Select(option => $"{Formatter.Bold(option.Name)}: {option.VotesCount} {translations.votes.ToLower()}"));
                }
                else
                {
                    options.ForEach(option =>
                    {
                        votes += $"{Formatter.Bold(option.Name)}: {option.VotesCount} {translations.votes.ToLower()}";
                        if (option.VotesCount > 0)
                        {
                            votes += $"\n{string.Join("\n", option.Voters.Select(member => $"<@{member}>"))}";
                        }
                        votes += "\n\n";
                    });
                }

                embed.WithTitle($"{translations.poll_finished}: {poll.Title}");
                embed.WithDescription(votes.NormalizeDescription());
                embed.WithColor(Constants.YumikoColor);
            }
            else
            {
                embed.WithTitle($"{translations.poll_finished}: {poll.Title}");
                embed.WithDescription(translations.no_one_has_voted);
                embed.WithColor(DiscordColor.Red);
            }

            return embed;
        }
    }
}
