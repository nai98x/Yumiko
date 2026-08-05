using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Yumiko.Infrastructure.Http;

/// <summary>
/// Same rate limit policy as the AniList executor, for the plain HTTP clients: it waits before
/// sending when a previous response left the window depleted, and it retries 429 and 5xx honouring
/// the <c>Retry-After</c> the API reports.
/// </summary>
internal sealed class RateLimitHandler(RateLimitState state, ILogger<RateLimitHandler> logger, string apiName)
    : DelegatingHandler
{
    private const int MaxRetryAttempts = 3;

    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(static response => IsTransient(response.StatusCode)),
                // On 429 we respect the Retry-After of the API; on the rest we use the default
                // exponential backoff (returning null).
                DelayGenerator = args => ValueTask.FromResult(
                    args.Outcome.Result is { StatusCode: HttpStatusCode.TooManyRequests } tooMany
                        ? state.RetryAfter(tooMany.Headers)
                        : (TimeSpan?)null),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Retrying {Api} request (attempt {Attempt}) after waiting {Delay}",
                        apiName,
                        args.AttemptNumber + 1,
                        args.RetryDelay);
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        await _pipeline.ExecuteAsync(
            async token =>
            {
                await WaitForWindowAsync(token);

                HttpResponseMessage response = await base.SendAsync(request, token);
                state.Update(response.Headers);

                return response;
            },
            cancellationToken);

    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests || (int)status >= 500;

    /// <summary>
    /// If a previous response left the window depleted, waits until the reset before continuing.
    /// Several concurrent requests wait until the same instant and then carry on.
    /// </summary>
    private async Task WaitForWindowAsync(CancellationToken token)
    {
        if (state.PausedUntil is not { } resetAt) return;

        TimeSpan wait = resetAt - DateTimeOffset.UtcNow;
        if (wait > TimeSpan.Zero)
        {
            logger.LogWarning("{Api} rate limit depleted; waiting {Wait} until the window resets.", apiName, wait);
            await Task.Delay(wait, token);
        }

        state.ClearPause(resetAt);
    }
}
