namespace Yumiko.Model.Entities;

public sealed class GameCharacterPage
{
    public List<CharacterOld> Characters { get; init; } = [];

    public bool HasNextPage { get; init; }
}
