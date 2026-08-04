using DSharpPlus;
using DSharpPlus.Entities;
using Yumiko.Bot.Games;
using Yumiko.Bot.Localization;

namespace Yumiko.Bot.Helpers;

public static class PollEmbeds
{
    public static DiscordEmbedBuilder Results(Poll poll, bool anonymous, Loc loc)
    {
        if (poll.TotalVotes == 0)
        {
            return new DiscordEmbedBuilder
            {
                Title = $"{loc[Keys.poll_finished]}: {poll.Title}",
                Description = loc[Keys.no_one_has_voted],
                Color = DiscordColor.Red,
            };
        }

        IEnumerable<string> lines = poll.Options
            .OrderByDescending(poll.Votes)
            .Select(option => Line(poll, option, anonymous, loc));

        return new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.poll_finished]}: {poll.Title}",
            Description = string.Join(anonymous ? "\n" : "\n\n", lines).NormalizeDescription(),
            Color = YumikoColors.Primary,
        };
    }

    private static string Line(Poll poll, string option, bool anonymous, Loc loc)
    {
        int votes = poll.Votes(option);
        string line = $"{Formatter.Bold(option)}: {votes} {loc[Keys.votes].ToLower(loc.Culture)}";

        if (anonymous || votes == 0)
        {
            return line;
        }

        return $"{line}\n{string.Join("\n", poll.Voters(option).Select(id => $"<@{id}>"))}";
    }
}
