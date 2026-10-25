using System.Collections.Concurrent;
using DSharpPlus.Entities;
using Yumiko.Application.Games;
using Yumiko.Model.Enum;

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

    public TriviaPlayers Players { get; set; }

    // Only in multiplayer: who signed up before the start, and thus who can answer.
    public TriviaLobby? Lobby { get; set; }

    public QuizRound? CurrentRound { get; set; }

    // The buttons carry an opaque option id, so the display name of every option played so far is kept
    // here: buttons of past rounds stay clickable and their attempt still has to be shown by name.
    public ConcurrentDictionary<string, string> OptionNames { get; } = new();

    public DiscordUser? CreatedBy { get; set; }

    public bool Canceled { get; set; }
}

public sealed class QuizRound
{
    public required TriviaRoundState State { get; init; }

    // Set by the component handler only after the interaction has been answered, so the runner does not
    // start posting followups while the response is still in flight.
    public bool Resolved { get; set; }

    public DiscordUser? Guesser { get; set; }

    public DiscordUser? Loser { get; set; }

    public string? MissedAttempt { get; set; }

    public DateTimeOffset GuessTime { get; set; }
}

public sealed class GameUser
{
    public required DiscordUser User { get; set; }

    public int Score { get; set; }

    public TimeSpan TotalTime { get; set; }
}
