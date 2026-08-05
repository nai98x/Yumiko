namespace Yumiko.Model.Exceptions;

/// <summary>
/// Base exception for any failure while querying the AniList API. The layer consuming
/// <c>IAnilistClient</c> can catch this type (or its derivatives) without knowing transport
/// details of the GraphQL/HTTP layer.
/// </summary>
public class AnilistApiException : Exception
{
    public AnilistApiException(string message) : base(message) { }

    public AnilistApiException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when AniList answers 404. It means the queried resource does not exist (for instance a
/// user with no list entry for a media), not that the request was malformed.
/// </summary>
public class AnilistNotFoundException(Exception innerException)
    : AnilistApiException("AniList answered HTTP 404; the queried resource does not exist.", innerException);

/// <summary>
/// Thrown when AniList answers with a server error (5xx) after the retries are exhausted.
/// It usually means the API is down or degraded, not a problem with the query.
/// </summary>
public class AnilistServerErrorException : AnilistApiException
{
    public int StatusCode { get; }

    public AnilistServerErrorException(int statusCode, Exception innerException)
        : base($"AniList answered HTTP {statusCode}; the API is probably down.", innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when AniList answers 429 (Too Many Requests) and the retries are exhausted.
/// <see cref="RetryAfter"/> tells how long to wait before trying again, when the
/// <c>Retry-After</c> header reported it.
/// </summary>
public class AnilistRateLimitException : AnilistApiException
{
    public TimeSpan? RetryAfter { get; }

    public AnilistRateLimitException(TimeSpan? retryAfter, Exception innerException)
        : base(BuildMessage(retryAfter), innerException)
    {
        RetryAfter = retryAfter;
    }

    private static string BuildMessage(TimeSpan? retryAfter) =>
        retryAfter is { } ra
            ? $"AniList hit the rate limit. Retry in {ra.TotalSeconds:0}s."
            : "AniList hit the rate limit.";
}
