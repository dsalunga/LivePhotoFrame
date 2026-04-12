using System.Text.Json;
using LivePhotoFrame.Maui.Models;

namespace LivePhotoFrame.Maui.Services;

/// <summary>
/// Manages app configuration persistence using MAUI Preferences.
/// </summary>
public class AppConfigService
{
    private const string ConfigKey = "AppConfig";

    public AppConfig Load()
    {
        var json = Preferences.Get(ConfigKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new AppConfig();

        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public void Save(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        Preferences.Set(ConfigKey, json);
    }
}
