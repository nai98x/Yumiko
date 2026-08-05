using Yumiko.Model.Enum;

namespace Yumiko.Model.Interfaces;

public interface IAnimalImageClient
{
    /// <summary>Downloads a random image of the requested animal.</summary>
    /// <returns><c>null</c> if the API did not return any.</returns>
    Task<byte[]?> GetRandomImageAsync(AnimalKind kind, CancellationToken cancellationToken = default);
}
