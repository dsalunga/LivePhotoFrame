using System.Net;
using System.Text.Json;
using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Provides photos from an HTTP/HTTPS manifest endpoint.
/// HTTPS is required by default; plain HTTP requires explicit opt-in.
/// </summary>
public class HttpPhotoProvider : IPhotoProvider
{
    private AppConfig _config = new();
    private HttpClient _httpClient = new();
    private List<string> _imageUrls = [];
    private int _index;

    public int Count => _imageUrls.Count;
    public string CurrentFileName => _imageUrls.Count > 0 ? Path.GetFileName(new Uri(_imageUrls[_index]).AbsolutePath) : string.Empty;

    public async Task InitializeAsync(AppConfig config)
    {
        _config = config;
        var httpSource = _config.HttpSource;

        var handler = new HttpClientHandler
        {
            MaxAutomaticRedirections = httpSource.MaxRedirects,
            AllowAutoRedirect = true,
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(httpSource.TimeoutSeconds),
            MaxResponseContentBufferSize = httpSource.MaxResponseBytes,
        };

        if (!string.IsNullOrEmpty(httpSource.AuthorizationHeader))
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Authorization", httpSource.AuthorizationHeader);
        }

        var manifestUrl = httpSource.ManifestUrl;

        if (string.IsNullOrWhiteSpace(manifestUrl))
            throw new InvalidOperationException("HTTP manifest URL is not configured.");

        var uri = new Uri(manifestUrl);
        if (uri.Scheme == "http" && !httpSource.AllowInsecureHttp)
            throw new InvalidOperationException(
                "Plain HTTP is not allowed. Set AllowInsecureHttp=true for trusted LAN sources.");

        var response = await _httpClient.GetAsync(manifestUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var urls = JsonSerializer.Deserialize<List<string>>(content) ?? [];

        // Validate URLs: only allow HTTPS by default
        _imageUrls = urls.Where(url =>
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed)) return false;
            if (parsed.Scheme == "https") return true;
            if (parsed.Scheme == "http" && httpSource.AllowInsecureHttp) return true;
            return false;
        }).ToList();

        if (_config.Shuffle)
            Shuffle(_imageUrls);
    }

    public async Task<Stream?> NextStreamAsync()
    {
        if (_imageUrls.Count == 0) return null;
        _index = (_index + 1) % _imageUrls.Count;
        return await DownloadImageAsync();
    }

    public async Task<Stream?> PreviousStreamAsync()
    {
        if (_imageUrls.Count == 0) return null;
        _index = (_index - 1 + _imageUrls.Count) % _imageUrls.Count;
        return await DownloadImageAsync();
    }

    private async Task<Stream> DownloadImageAsync()
    {
        var url = _imageUrls[_index];
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        // Validate content type
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unexpected content type: {contentType}");

        var memStream = new MemoryStream();
        await response.Content.CopyToAsync(memStream);
        memStream.Position = 0;
        return memStream;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
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
