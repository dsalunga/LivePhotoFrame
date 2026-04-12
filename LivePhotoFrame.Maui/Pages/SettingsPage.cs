using LivePhotoFrame.Maui.Models;
using LivePhotoFrame.Maui.Services;

namespace LivePhotoFrame.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly AppConfigService _configService;

    public SettingsPage(AppConfigService configService)
    {
        InitializeComponent();
        _configService = configService;
        LoadConfig();
    }

    private void LoadConfig()
    {
        var config = _configService.Load();

        pickerSource.SelectedIndex = (int)config.ActiveSource;

        entryFtpHost.Text = config.Ftp.Hostname;
        entryFtpPath.Text = config.Ftp.Path;
        entryFtpUser.Text = config.Ftp.Username;
        entryFtpPass.Text = config.Ftp.Password;

        entryFsPath.Text = config.FileSystemPath;
        entryHttpManifestUrl.Text = config.HttpSource.ManifestUrl;

        entryInterval.Text = config.Interval.ToString();
        entryMaxIdle.Text = config.MaxIdleTime.ToString();

        pickerDisplayMode.SelectedIndex = (int)config.ImageDisplayMode;
        chkShuffle.IsChecked = config.Shuffle;
        chkSkipPortraits.IsChecked = config.SkipPortraits;
        chkAutoStart.IsChecked = config.AutoStartShow;
    }

    private AppConfig BuildConfig()
    {
        var config = _configService.Load();

        config.ActiveSource = (PhotoSourceType)(pickerSource.SelectedIndex < 0 ? 0 : pickerSource.SelectedIndex);

        config.Ftp.Hostname = entryFtpHost.Text ?? string.Empty;
        config.Ftp.Path = entryFtpPath.Text ?? string.Empty;
        config.Ftp.Username = entryFtpUser.Text ?? string.Empty;
        config.Ftp.Password = entryFtpPass.Text ?? string.Empty;

        config.FileSystemPath = entryFsPath.Text ?? string.Empty;
        config.HttpSource.ManifestUrl = entryHttpManifestUrl.Text ?? string.Empty;

        if (int.TryParse(entryInterval.Text, out var interval))
            config.Interval = interval;
        if (int.TryParse(entryMaxIdle.Text, out var maxIdle))
            config.MaxIdleTime = maxIdle;

        config.ImageDisplayMode = (ImageDisplayMode)(pickerDisplayMode.SelectedIndex < 0 ? 0 : pickerDisplayMode.SelectedIndex);
        config.Shuffle = chkShuffle.IsChecked;
        config.SkipPortraits = chkSkipPortraits.IsChecked;
        config.AutoStartShow = chkAutoStart.IsChecked;

        return config;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        _configService.Save(BuildConfig());
        await DisplayAlertAsync("Saved", "Settings have been saved.", "OK");
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        _configService.Save(BuildConfig());
        await Shell.Current.GoToAsync(nameof(SlideshowPage));
    }
}
