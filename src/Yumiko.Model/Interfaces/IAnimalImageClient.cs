using Yumiko.Model.Enum;

namespace Yumiko.Model.Interfaces;

public interface IAnimalImageClient
{
    /// <summary>Descarga una imagen al azar del animal pedido.</summary>
    /// <returns><c>null</c> si la API no devolvió ninguna.</returns>
    Task<byte[]?> GetRandomImageAsync(AnimalKind kind, CancellationToken cancellationToken = default);
}
