using System.Collections.Concurrent;
using DSharpPlus.Entities;

namespace Yumiko.Bot.Games;

/// <summary>
/// Ongoing trivia match. It lives in the Bot because it references Discord types: it is session state,
/// not domain state.
/// </summary>
public sealed class Trivia
{
    public string? Title { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public double TimeoutTotal { get; set; }

    public QuizRound CurrentRound { get; set; } = new();

    // The buttons carry an opaque option id, so the display name of every option played so far is kept
    // here: buttons of past rounds stay clickable and their attempt still has to be shown by name.
    public ConcurrentDictionary<string, string> OptionNames { get; } = new();

    public DiscordUser? CreatedBy { get; set; }

    public bool Canceled { get; set; }
}

public sealed class QuizRound
{
    public string Match { get; set; } = string.Empty;

    public double TimeoutCurrent { get; set; }

    public bool Guessed { get; set; }

    public DiscordUser? Guesser { get; set; }

    public DateTimeOffset GuessTime { get; set; }
}

public sealed class GameUser
{
    public required DiscordUser User { get; set; }

    public int Score { get; set; }
}
