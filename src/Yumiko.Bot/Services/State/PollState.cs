using System.Collections.Concurrent;
using Yumiko.Bot.Games;

namespace Yumiko.Bot.Services.State;

public sealed class PollState
{
    private readonly ConcurrentDictionary<string, Poll> _polls = new();

    public void Add(Poll poll) => _polls[poll.Id] = poll;

    public Poll? Get(string id) => _polls.TryGetValue(id, out Poll? poll) ? poll : null;

    public void Remove(string id) => _polls.TryRemove(id, out _);
}
