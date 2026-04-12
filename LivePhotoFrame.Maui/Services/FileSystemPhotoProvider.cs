using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Provides photos from the local file system.
/// </summary>
public class FileSystemPhotoProvider : IPhotoProvider
{
    private AppConfig _config = new();
    private List<string> _files = [];
    private int _index;

    public int Count => _files.Count;
    public string CurrentFileName => _files.Count > 0 ? Path.GetFileName(_files[_index]) : string.Empty;

    public Task InitializeAsync(AppConfig config)
    {
        _config = config;

        if (string.IsNullOrWhiteSpace(_config.FileSystemPath) || !Directory.Exists(_config.FileSystemPath))
            throw new InvalidOperationException($"Photo directory not found: {_config.FileSystemPath}");

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
        };

        _files = Directory.EnumerateFiles(_config.FileSystemPath)
            .Where(f => extensions.Contains(Path.GetExtension(f)))
            .ToList();

        if (_config.Shuffle)
            Shuffle(_files);

        _index = 0;
        return Task.CompletedTask;
    }

    public Task<Stream?> NextStreamAsync()
    {
        if (_files.Count == 0) return Task.FromResult<Stream?>(null);
        _index = (_index + 1) % _files.Count;
        return OpenFileAsync();
    }

    public Task<Stream?> PreviousStreamAsync()
    {
        if (_files.Count == 0) return Task.FromResult<Stream?>(null);
        _index = (_index - 1 + _files.Count) % _files.Count;
        return OpenFileAsync();
    }

    private Task<Stream?> OpenFileAsync()
    {
        var stream = File.OpenRead(_files[_index]);
        return Task.FromResult<Stream?>(stream);
    }

    public void Dispose()
    {
        // No persistent resources to clean up.
    }

    private static void Shuffle<T>(IList<T> list)
    {
        var rng = Random.Shared;
        for (int n = list.Count - 1; n > 0; n--)
        {
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
