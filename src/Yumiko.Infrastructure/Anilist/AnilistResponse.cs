using Yumiko.Model.Entities.Anilist;

namespace Yumiko.Infrastructure.Anilist;

/// <summary>
/// Internal result the executor returns to <c>AnilistClient</c>: the deserialized data
/// plus the rate limit state read from the headers of that same response.
/// </summary>
internal sealed record AnilistResponse<T>(T? Data, AnilistRateLimit RateLimit);
