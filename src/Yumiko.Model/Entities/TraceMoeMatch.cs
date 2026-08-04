namespace Yumiko.Model.Entities;

public class TraceMoeMatch
{
    public int AnilistId { get; init; }

    public string? Episode { get; init; }

    /// <summary>Similitud como fracción (0 a 1), tal cual la devuelve trace.moe.</summary>
    public double Similarity { get; init; }

    public double From { get; init; }

    public string? Video { get; init; }
}
