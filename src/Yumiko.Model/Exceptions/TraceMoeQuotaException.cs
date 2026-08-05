namespace Yumiko.Model.Exceptions;

/// <summary>
/// trace.moe answered 402 (monthly quota depleted) or 429 (too many searches in a row).
/// </summary>
public sealed class TraceMoeQuotaException(int statusCode)
    : Exception($"trace.moe answered HTTP {statusCode}: the search quota is depleted.")
{
    public int StatusCode { get; } = statusCode;
}
