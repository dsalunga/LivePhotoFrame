namespace LivePhotoFrame.Maui.Models;

public enum ImageDisplayMode
{
    Uniform,
    UniformToFill,
    BestFit
}

public enum PhotoSourceType
{
    FileSystem,
    FTP,
    Http
}

public class AppConfig
{
    public FtpConfig Ftp { get; set; } = new();
    public HttpSourceConfig HttpSource { get; set; } = new();
    public string FileSystemPath { get; set; } = string.Empty;
    public PhotoSourceType ActiveSource { get; set; } = PhotoSourceType.FileSystem;
    public bool AutoStartShow { get; set; }
    public bool SkipPortraits { get; set; }
    public bool Shuffle { get; set; } = true;

    /// <summary>
    /// Interval in minutes between photo transitions.
    /// </summary>
    public int Interval { get; set; } = 5;

    /// <summary>
    /// Maximum idle time in minutes to prevent burn-in.
    /// </summary>
    public int MaxIdleTime { get; set; } = 60;

    public ImageDisplayMode ImageDisplayMode { get; set; } = ImageDisplayMode.BestFit;
}

public class FtpConfig
{
    public string Hostname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Path { get; set; } = "/";
}

public class HttpSourceConfig
{
    /// <summary>
    /// Base URL for the image manifest/feed endpoint.
    /// </summary>
    public string ManifestUrl { get; set; } = string.Empty;

    /// <summary>
    /// Allow plain HTTP connections (default: false, HTTPS only).
    /// </summary>
    public bool AllowInsecureHttp { get; set; }

    /// <summary>
    /// Optional auth header value (e.g., "Bearer token123").
    /// </summary>
    public string? AuthorizationHeader { get; set; }

    /// <summary>
    /// Maximum response size in bytes (default: 50 MB).
    /// </summary>
    public long MaxResponseBytes { get; set; } = 50 * 1024 * 1024;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of HTTP redirects to follow.
    /// </summary>
    public int MaxRedirects { get; set; } = 5;
}
