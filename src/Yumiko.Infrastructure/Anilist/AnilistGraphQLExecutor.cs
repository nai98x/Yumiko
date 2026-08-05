using System.Net;
using System.Net.Http.Headers;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Exceptions;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Yumiko.Infrastructure.Anilist;

/// <summary>
/// Core of the AniList access. It owns a single reused <see cref="GraphQLHttpClient"/> and
/// centralizes resiliency: retries with backoff (Polly), waiting for <c>Retry-After</c> on
/// 429, translation of transport errors into domain exceptions and rate limit reading.
/// It is a singleton; the concrete query methods live in <see cref="AnilistClient"/>.
/// </summary>
internal sealed class AnilistGraphQLExecutor : IDisposable
{
    private const string Endpoint = "https://graphql.anilist.co";

    private readonly GraphQLHttpClient _client;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<AnilistGraphQLExecutor> _logger;

    // Proactive wait: when a response reports there are no requests left in the window
    // (Remaining <= 0), we store until when we have to hold off so we do not cause a 429.
    private readonly object _rateLimitLock = new();
    private DateTimeOffset? _pausedUntil;

    public AnilistGraphQLExecutor(ILogger<AnilistGraphQLExecutor> logger)
    {
        _logger = logger;
        _client = new GraphQLHttpClient(
            new GraphQLHttpClientOptions { EndPoint = new Uri(Endpoint) },
            new NewtonsoftJsonSerializer(),
            new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
        _pipeline = BuildPipeline();
    }
    
    /// <summary>
    /// Same as <see cref="SendQueryAsync{T}"/> but attaching the user OAuth token in the
    /// <c>Authorization</c> header (queries over <c>Viewer</c> and other private data).
    /// </summary>
    public Task<AnilistResponse<T>> SendAuthenticatedQueryAsync<T>(GraphQLRequest request, string accessToken, CancellationToken cancellationToken) =>
        SendQueryAsync<T>(new AuthenticatedGraphQLHttpRequest(request, accessToken), cancellationToken);

    /// <summary>
    /// Runs a query and returns the typed data along with the rate limit state. It applies the
    /// retry pipeline and translates failures into <see cref="AnilistApiException"/> and derivatives.
    /// </summary>
    public async Task<AnilistResponse<T>> SendQueryAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                // Before every attempt (retries included), respect the window if it is depleted.
                await WaitForRateLimitWindowAsync(token);

                GraphQLResponse<T> response = await _client.SendQueryAsync<T>(request, token);

                // GraphQL level errors (HTTP 200 with an "errors" array): invalid query, nonexistent
                // media, etc. They are not transient, so they are not retried.
                if (response.Errors is { Length: > 0 })
                {
                    string detail = string.Join("; ", response.Errors.Select(e => e.Message));
                    throw new AnilistApiException($"AniList returned errors: {detail}");
                }

                GraphQLHttpResponse<T> http = response.AsGraphQLHttpResponse();
                AnilistRateLimit rateLimit = ReadRateLimit(http.ResponseHeaders);
                UpdateRateLimitState(rateLimit);
                return new AnilistResponse<T>(response.Data, rateLimit);
            }, cancellationToken);
        }
        catch (GraphQLHttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new AnilistRateLimitException(GetRetryAfter(ex.ResponseHeaders), ex);
        }
        catch (GraphQLHttpRequestException ex) when ((int)ex.StatusCode >= 500)
        {
            throw new AnilistServerErrorException((int)ex.StatusCode, ex);
        }
        catch (GraphQLHttpRequestException ex)
        {
            throw new AnilistApiException($"AniList answered HTTP {(int)ex.StatusCode} ({ex.StatusCode}).", ex);
        }
    }

    private ResiliencePipeline BuildPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<GraphQLHttpRequestException>(IsTransient),
                // On 429 we respect the AniList Retry-After; on the rest we use the default
                // exponential backoff (returning null).
                DelayGenerator = static args =>
                {
                    if (args.Outcome.Exception is GraphQLHttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } ex)
                    {
                        return ValueTask.FromResult(GetRetryAfter(ex.ResponseHeaders));
                    }

                    return ValueTask.FromResult<TimeSpan?>(null);
                },
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        args.Outcome.Exception,
                        "Reintentando request a AniList (intento {Attempt}) tras esperar {Delay}",
                        args.AttemptNumber + 1,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

    /// <summary>
    /// If a previous response left the rate limit window depleted, waits until the reset before
    /// continuing. Several concurrent queries wait until the same instant and then carry on.
    /// </summary>
    private async Task WaitForRateLimitWindowAsync(CancellationToken token)
    {
        DateTimeOffset? until;
        lock (_rateLimitLock) until = _pausedUntil;

        if (until is not { } resetAt) return;

        TimeSpan wait = resetAt - DateTimeOffset.UtcNow;
        if (wait > TimeSpan.Zero)
        {
            _logger.LogWarning("AniList rate limit depleted; waiting {Wait} until the window resets.", wait);
            await Task.Delay(wait, token);
        }

        // Clear the pause only if nobody moved it further into the future while we were waiting.
        lock (_rateLimitLock)
        {
            if (_pausedUntil == until) _pausedUntil = null;
        }
    }

    /// <summary>
    /// Registers the proactive pause when the response reports there are no requests left in the window.
    /// A 1s margin is added to tolerate clock drift against the reset reported by AniList.
    /// </summary>
    private void UpdateRateLimitState(AnilistRateLimit rateLimit)
    {
        if (rateLimit is { Remaining: <= 0, ResetAt: { } reset })
        {
            lock (_rateLimitLock) _pausedUntil = reset.AddSeconds(1);
        }
    }

    /// <summary>HTTP errors worth retrying: rate limit (429) and server errors (5xx).</summary>
    private static bool IsTransient(GraphQLHttpRequestException ex) =>
        ex.StatusCode == HttpStatusCode.TooManyRequests || (int)ex.StatusCode >= 500;

    private static TimeSpan? GetRetryAfter(HttpResponseHeaders? headers)
    {
        RetryConditionHeaderValue? retryAfter = headers?.RetryAfter;
        if (retryAfter is null) return null;

        if (retryAfter.Delta is { } delta) return delta;
        if (retryAfter.Date is { } date)
        {
            TimeSpan wait = date - DateTimeOffset.UtcNow;
            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return null;
    }

    private static AnilistRateLimit ReadRateLimit(HttpResponseHeaders headers)
    {
        AnilistRateLimit rateLimit = new();

        if (TryGetInt(headers, "X-RateLimit-Limit", out int limit)) rateLimit.Limit = limit;
        if (TryGetInt(headers, "X-RateLimit-Remaining", out int remaining)) rateLimit.Remaining = remaining;
        // AniList reports the reset as an epoch in seconds (Unix time).
        if (TryGetLong(headers, "X-RateLimit-Reset", out long reset)) rateLimit.ResetAt = DateTimeOffset.FromUnixTimeSeconds(reset);

        return rateLimit;
    }

    private static bool TryGetInt(HttpResponseHeaders headers, string name, out int value)
    {
        value = 0;
        return headers.TryGetValues(name, out IEnumerable<string>? values)
               && int.TryParse(values.FirstOrDefault(), out value);
    }

    private static bool TryGetLong(HttpResponseHeaders headers, string name, out long value)
    {
        value = 0;
        return headers.TryGetValues(name, out IEnumerable<string>? values)
               && long.TryParse(values.FirstOrDefault(), out value);
    }

    public void Dispose() => _client.Dispose();
}
