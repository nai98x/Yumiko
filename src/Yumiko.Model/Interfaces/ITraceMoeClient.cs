using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces;

public interface ITraceMoeClient
{
    /// <summary>Busca el anime de una imagen. Los resultados vienen ordenados por similitud.</summary>
    Task<List<TraceMoeMatch>> SearchAsync(string imageUrl, CancellationToken cancellationToken = default);
}
