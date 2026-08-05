using System.Collections.Concurrent;

namespace Yumiko.Bot.Games;

/// <summary>
/// Ongoing poll. The votes go in a userId → option dictionary: that way a user always has
/// one vote and changing it overwrites the previous one atomically.
/// </summary>
public sealed class Poll
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required IReadOnlyList<string> Options { get; init; }

    private readonly ConcurrentDictionary<ulong, string> _votes = new();

    /// <summary>Registers the vote. Returns <c>false</c> if the user had already voted for that same option.</summary>
    public bool Vote(ulong userId, string option)
    {
        if (!Options.Contains(option))
        {
            return false;
        }

        string? previous = _votes.TryGetValue(userId, out string? v) ? v : null;
        if (previous == option)
        {
            return false;
        }

        _votes[userId] = option;
        return true;
    }

    public int Votes(string option) => _votes.Count(kv => kv.Value == option);

    public IReadOnlyList<ulong> Voters(string option) =>
        [.. _votes.Where(kv => kv.Value == option).Select(kv => kv.Key)];

    public int TotalVotes => _votes.Count;
}
