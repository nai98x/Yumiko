namespace Yumiko.Model.Entities.Anilist;

/// <summary>
/// State of the AniList API rate limit, read from the <c>X-RateLimit-*</c> headers of the last
/// response. The values are nullable because not every response includes
/// those headers.
/// </summary>
public class AnilistRateLimit
{
    /// <summary>Maximum amount of requests allowed in the current window.</summary>
    public int? Limit { get; set; }

    /// <summary>Requests left in the current window.</summary>
    public int? Remaining { get; set; }

    /// <summary>Moment the window resets (when the header reports it).</summary>
    public DateTimeOffset? ResetAt { get; set; }
}
