using DSharpPlus.Entities;

namespace Yumiko.Bot.Games;

/// <summary>
/// Partida de trivia en curso. Vive en el Bot porque referencia tipos de Discord: es estado de sesión,
/// no de dominio.
/// </summary>
public sealed class Trivia
{
    public string? Title { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public double TimeoutTotal { get; set; }

    public QuizRound CurrentRound { get; set; } = new();

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
