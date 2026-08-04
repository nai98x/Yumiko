namespace Yumiko.Model.Exceptions;

/// <summary>
/// trace.moe respondió 402 (cuota mensual agotada) o 429 (demasiadas búsquedas seguidas).
/// </summary>
public sealed class TraceMoeQuotaException(int statusCode)
    : Exception($"trace.moe respondió HTTP {statusCode}: se agotó la cuota de búsquedas.")
{
    public int StatusCode { get; } = statusCode;
}
