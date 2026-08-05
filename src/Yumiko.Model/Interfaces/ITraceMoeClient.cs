using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces;

public interface ITraceMoeClient
{
    /// <summary>Searches for the anime of an image. Results come sorted by similarity.</summary>
    Task<List<TraceMoeMatch>> SearchAsync(string imageUrl, CancellationToken cancellationToken = default);
}
