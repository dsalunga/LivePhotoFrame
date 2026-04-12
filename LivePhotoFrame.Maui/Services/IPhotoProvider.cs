using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Abstraction for photo sources (file system, FTP, HTTP).
/// </summary>
public interface IPhotoProvider : IDisposable
{
    int Count { get; }
    string CurrentFileName { get; }
    Task InitializeAsync(AppConfig config);
    Task<Stream?> NextStreamAsync();
    Task<Stream?> PreviousStreamAsync();
}
