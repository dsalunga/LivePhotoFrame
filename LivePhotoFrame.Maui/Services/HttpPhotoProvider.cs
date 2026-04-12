using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Provides photos from an HTTP/HTTPS manifest endpoint.
/// HTTPS is required by default; plain HTTP requires explicit opt-in.
/// Supports ETag/Last-Modified caching and retry with exponential backoff.
/// </summary>
public class HttpPhotoProvider : IPhotoProvider
{
    private AppConfig _config = new();
    private HttpClient _httpClient = new();
    private List<string> _imageUrls = [];
    private int _index;

    // Cache: URL -> (ETag, LastModified, CachedBytes)
    private readonly Dictionary<string, CacheEntry> _cache = new();

    private const int MaxRetries = 3;
    private static readonly TimeSpan InitialBackoff = TimeSpan.FromSeconds(1);

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

        var response = await SendWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, manifestUrl));
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

        var response = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add conditional headers from cache
            if (_cache.TryGetValue(url, out var entry))
            {
                if (entry.ETag is not null)
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(entry.ETag));
                if (entry.LastModified.HasValue)
                    request.Headers.IfModifiedSince = entry.LastModified.Value;
            }

            return request;
        });

        // Return cached version on 304 Not Modified
        if (response.StatusCode == HttpStatusCode.NotModified && _cache.TryGetValue(url, out var cached))
        {
            var memStream = new MemoryStream(cached.Data);
            return memStream;
        }

        response.EnsureSuccessStatusCode();

        // Validate content type
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unexpected content type: {contentType}");

        var data = await response.Content.ReadAsByteArrayAsync();

        // Update cache with ETag/Last-Modified
        var etag = response.Headers.ETag?.ToString();
        var lastModified = response.Content.Headers.LastModified;
        if (etag is not null || lastModified.HasValue)
        {
            _cache[url] = new CacheEntry(etag, lastModified, data);
        }

        return new MemoryStream(data);
    }

    /// <summary>
    /// Sends an HTTP request with retry and exponential backoff on transient failures.
    /// </summary>
    private async Task<HttpResponseMessage> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory)
    {
        var backoff = InitialBackoff;
        for (int attempt = 0; ; attempt++)
        {
            var request = requestFactory();
            try
            {
                var response = await _httpClient.SendAsync(request);

                // Retry on server errors (5xx) and 429 Too Many Requests
                if (attempt < MaxRetries && ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.TooManyRequests))
                {
                    response.Dispose();
                    await Task.Delay(backoff);
                    backoff *= 2;
                    continue;
                }

                return response;
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                await Task.Delay(backoff);
                backoff *= 2;
            }
            catch (TaskCanceledException) when (attempt < MaxRetries)
            {
                await Task.Delay(backoff);
                backoff *= 2;
            }
        }
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

    private sealed record CacheEntry(string? ETag, DateTimeOffset? LastModified, byte[] Data);
}
