using System.Net.Http.Headers;

namespace Yumiko.Infrastructure.Http;

/// <summary>
/// Proactive wait shared by every request of an API: when a response reports there are no requests
/// left in the window, it stores until when to hold off so the next call does not cause a 429.
/// It is a singleton because the state has to outlive the handler instances the factory rotates.
/// </summary>
internal sealed class RateLimitState(TimeSpan window)
{
    private readonly object _lock = new();
    private DateTimeOffset? _pausedUntil;

    /// <summary>Instant the window resets, while the pause is in effect.</summary>
    public DateTimeOffset? PausedUntil
    {
        get
        {
            lock (_lock) return _pausedUntil;
        }
    }

    /// <summary>Clears the pause only if nobody moved it further into the future meanwhile.</summary>
    public void ClearPause(DateTimeOffset waitedFor)
    {
        lock (_lock)
        {
            if (_pausedUntil == waitedFor) _pausedUntil = null;
        }
    }

    /// <summary>
    /// Registers the pause when the response leaves the window depleted. A 1s margin is added to
    /// tolerate clock drift against the reset reported by the API.
    /// </summary>
    public void Update(HttpResponseHeaders headers)
    {
        // Without the header there is nothing to go by: the API does not report its quota.
        if (ReadLong(headers, "X-RateLimit-Remaining") is not <= 0) return;

        DateTimeOffset until = DateTimeOffset.UtcNow.Add(RetryAfter(headers)).AddSeconds(1);

        lock (_lock)
        {
            if (_pausedUntil is null || until > _pausedUntil) _pausedUntil = until;
        }
    }

    /// <summary>
    /// Wait the API asks for: <c>Retry-After</c> first, then <c>X-RateLimit-Reset</c>, and the whole
    /// window when the response reports neither.
    /// </summary>
    public TimeSpan RetryAfter(HttpResponseHeaders headers)
    {
        if (headers.RetryAfter?.Delta is { } delta) return delta;

        if (headers.RetryAfter?.Date is { } date)
        {
            TimeSpan wait = date - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero) return wait;
        }

        if (ReadResetAt(headers) is { } resetAt)
        {
            TimeSpan wait = resetAt - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero) return wait;
        }

        return window;
    }

    // The reset is reported as an epoch, in seconds or in milliseconds depending on the API.
    private static DateTimeOffset? ReadResetAt(HttpResponseHeaders headers) =>
        ReadLong(headers, "X-RateLimit-Reset") is { } reset
            ? reset > 100_000_000_000
                ? DateTimeOffset.FromUnixTimeMilliseconds(reset)
                : DateTimeOffset.FromUnixTimeSeconds(reset)
            : null;

    private static long? ReadLong(HttpResponseHeaders headers, string name) =>
        headers.TryGetValues(name, out IEnumerable<string>? values)
            && long.TryParse(values.FirstOrDefault(), out long value)
                ? value
                : null;
}
