using System.Collections.Concurrent;
using Yumiko.Bot.Games;

namespace Yumiko.Bot.Services.State;

/// <summary>
/// Trivias en curso, indexadas por (guild, canal). Se muta desde hilos del gateway, así que va sobre
/// un <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class TriviaState
{
    private readonly ConcurrentDictionary<(ulong GuildId, ulong ChannelId), Trivia> _trivias = new();

    public bool TryAdd(Trivia trivia) => _trivias.TryAdd((trivia.GuildId, trivia.ChannelId), trivia);

    public Trivia? Get(ulong guildId, ulong channelId) =>
        _trivias.TryGetValue((guildId, channelId), out Trivia? trivia) ? trivia : null;

    public void Remove(ulong guildId, ulong channelId) => _trivias.TryRemove((guildId, channelId), out _);

    public void UpdateCurrentRound(ulong guildId, ulong channelId, QuizRound round)
    {
        if (_trivias.TryGetValue((guildId, channelId), out Trivia? trivia))
        {
            trivia.CurrentRound = round;
        }
    }
}
