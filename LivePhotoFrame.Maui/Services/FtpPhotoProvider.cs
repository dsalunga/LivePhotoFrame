using FluentFTP;
using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Provides photos from an FTP server with local caching.
/// </summary>
public class FtpPhotoProvider : IPhotoProvider
{
    private AppConfig _config = new();
    private AsyncFtpClient? _client;
    private FtpListItem[] _items = [];
    private int _index;

    public int Count => _items.Length;
    public string CurrentFileName => _items.Length > 0 ? _items[_index].Name : string.Empty;

    public async Task InitializeAsync(AppConfig config)
    {
        _config = config;
        var ftpConfig = _config.Ftp;
        _client = new AsyncFtpClient(ftpConfig.Hostname, ftpConfig.Username, ftpConfig.Password);
        await _client.Connect();

        _items = await _client.GetListing(ftpConfig.Path);
        if (_config.Shuffle)
            Shuffle(_items);
    }

    public async Task<Stream?> NextStreamAsync()
    {
        if (_items.Length == 0) return null;
        _index = (_index + 1) % _items.Length;
        return await ReadStreamAsync();
    }

    public async Task<Stream?> PreviousStreamAsync()
    {
        if (_items.Length == 0) return null;
        _index = (_index - 1 + _items.Length) % _items.Length;
        return await ReadStreamAsync();
    }

    private async Task<Stream> ReadStreamAsync()
    {
        if (_client == null || !_client.IsConnected)
        {
            _client ??= new AsyncFtpClient(
                _config.Ftp.Hostname,
                _config.Ftp.Username,
                _config.Ftp.Password);
            await _client.Connect();
        }

        var file = _items[_index];
        var memStream = new MemoryStream();
        await _client.DownloadStream(memStream, file.FullName);
        memStream.Position = 0;
        return memStream;
    }

    public void Dispose()
    {
        _client?.Disconnect();
        _client?.Dispose();
    }

    private static void Shuffle<T>(T[] array)
    {
        var rng = Random.Shared;
        for (int n = array.Length - 1; n > 0; n--)
        {
            int k = rng.Next(n + 1);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }
}
