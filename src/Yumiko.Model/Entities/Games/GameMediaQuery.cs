using Yumiko.Model.Enum;

namespace Yumiko.Model.Entities;

/// <summary>Filters of the media pool that feeds the games.</summary>
public sealed record GameMediaQuery
{
    public required MediaType Type { get; init; }

    /// <summary>If set, filters by genre and sorts by popularity instead of by favourites.</summary>
    public string? Genre { get; init; }

    public bool IncludeAdult { get; init; }

    public bool IncludeCharacters { get; init; }

    public bool IncludeStudios { get; init; }

    public bool IncludeRelatedMedia { get; init; }

    /// <summary>Excludes media that has not aired yet (used by the Higher or Lower cache).</summary>
    public bool ExcludeUnreleased { get; init; }
}

public sealed class GameMediaPage
{
    public List<Anime> Media { get; init; } = [];

    public bool HasNextPage { get; init; }

    public int LastPage { get; init; }
}
