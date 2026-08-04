using System.Collections.Concurrent;

namespace Yumiko.Bot.Games;

/// <summary>
/// Encuesta en curso. Los votos van en un diccionario userId → opción: así un usuario tiene siempre
/// un voto y cambiarlo pisa el anterior de forma atómica.
/// </summary>
public sealed class Poll
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required IReadOnlyList<string> Options { get; init; }

    private readonly ConcurrentDictionary<ulong, string> _votes = new();

    /// <summary>Registra el voto. Devuelve <c>false</c> si el usuario ya había votado esa misma opción.</summary>
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
