using System.Collections.Concurrent;

namespace Yumiko.Bot.Services.State;

/// <summary>Contador de usos por comando desde que arrancó el proceso (lo muestra <c>/owner commands</c>).</summary>
public sealed class CommandUsageState
{
    private readonly ConcurrentDictionary<string, int> _uses = new();

    public void Increment(string commandName) => _uses.AddOrUpdate(commandName, 1, (_, current) => current + 1);

    public IReadOnlyList<(string CommandName, int Uses)> Snapshot() =>
        [.. _uses.Select(kv => (kv.Key, kv.Value)).OrderByDescending(x => x.Value)];
}
