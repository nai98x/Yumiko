namespace Yumiko.Model.Exceptions;

/// <summary>
/// trace.moe could not download the image of the given link (dead link, expired signature or a host
/// that blocks it). It is a problem with the link, not with the search.
/// </summary>
public sealed class TraceMoeImageFetchException(int statusCode, string detail)
    : Exception($"trace.moe answered HTTP {statusCode}: {detail}")
{
    public int StatusCode { get; } = statusCode;
}
