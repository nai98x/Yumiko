namespace Yumiko.Model.Entities;

public class TraceMoeMatch
{
    public int AnilistId { get; init; }

    public string? Episode { get; init; }

    /// <summary>Similarity as a fraction (0 to 1), exactly as trace.moe returns it.</summary>
    public double Similarity { get; init; }

    public double From { get; init; }

    public string? Video { get; init; }
}
