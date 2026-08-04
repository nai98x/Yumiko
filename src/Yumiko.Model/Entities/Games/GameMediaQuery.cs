using Yumiko.Model.Enum;

namespace Yumiko.Model.Entities;

/// <summary>Filtros del pool de medias que alimenta los juegos.</summary>
public sealed record GameMediaQuery
{
    public required MediaType Type { get; init; }

    /// <summary>Si se define, filtra por género y ordena por popularidad en vez de por favoritos.</summary>
    public string? Genre { get; init; }

    public bool IncludeAdult { get; init; }

    public bool IncludeCharacters { get; init; }

    public bool IncludeStudios { get; init; }

    public bool IncludeRelatedMedia { get; init; }

    /// <summary>Excluye los medias que todavía no salieron (se usa para el caché de Higher or Lower).</summary>
    public bool ExcludeUnreleased { get; init; }
}

public sealed class GameMediaPage
{
    public List<Anime> Media { get; init; } = [];

    public bool HasNextPage { get; init; }

    public int LastPage { get; init; }
}
