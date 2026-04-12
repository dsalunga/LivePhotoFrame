using LivePhotoFrame.Maui.Models;
using LivePhotoFrame.Maui.Services;

namespace LivePhotoFrame.Maui.Pages;

public partial class SlideshowPage : ContentPage
{
    private readonly AppConfigService _configService;
    private IPhotoProvider? _provider;
    private IDispatcherTimer? _timer;
    private AppConfig _config = new();
    private int _totalIdleMinutes;
    private bool _displaying;

    public SlideshowPage(AppConfigService configService)
    {
        InitializeComponent();
        _configService = configService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _config = _configService.Load();

        _provider = _config.ActiveSource switch
        {
            PhotoSourceType.FTP => new FtpPhotoProvider(),
            PhotoSourceType.Http => new HttpPhotoProvider(),
            _ => new FileSystemPhotoProvider(),
        };

        try
        {
            await _provider.InitializeAsync(_config);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        if (_provider.Count == 0)
        {
            await DisplayAlertAsync("Error", "No photos found.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        // Swipe gestures
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (_, _) => DisplayPhoto(previous: false, restartTimer: true);
        Content.GestureRecognizers.Add(swipeLeft);

        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (_, _) => DisplayPhoto(previous: true, restartTimer: true);
        Content.GestureRecognizers.Add(swipeRight);

        // Double-tap to go back
        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += async (_, _) => await Shell.Current.GoToAsync("..");
        Content.GestureRecognizers.Add(doubleTap);

        // Initial display
        DisplayPhoto();

        // Timer
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMinutes(_config.Interval);
        _timer.Tick += (_, _) => DisplayPhoto();
        _timer.Start();

#if WINDOWS || MACCATALYST
        // Keep screen on via DeviceDisplay
        DeviceDisplay.Current.KeepScreenOn = true;
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _provider?.Dispose();

#if WINDOWS || MACCATALYST
        DeviceDisplay.Current.KeepScreenOn = false;
#endif
    }

    private async void DisplayPhoto(bool previous = false, bool restartTimer = false)
    {
        if (_displaying || _provider is null) return;
        _displaying = true;

        try
        {
            int portraitSkipped = 0;
            do
            {
                using var stream = await (previous
                    ? _provider.PreviousStreamAsync()
                    : _provider.NextStreamAsync());

                if (stream is null) break;

                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                var source = ImageSource.FromStream(() => memStream);

                // Determine if portrait (simple heuristic - not available without decode on MAUI;
                // skip-portrait uses filename hint from provider)
                photoImage.Source = source;
                photoImage.Aspect = _config.ImageDisplayMode switch
                {
                    ImageDisplayMode.UniformToFill => Aspect.AspectFill,
                    _ => Aspect.AspectFit,
                };

                portraitSkipped = 0; // Displayed successfully
            }
            while (portraitSkipped > 0 && portraitSkipped < _provider.Count);

            _totalIdleMinutes = 0;
            if (restartTimer && _timer is not null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }
        catch (Exception)
        {
            _totalIdleMinutes += _config.Interval;
            if (_totalIdleMinutes >= _config.MaxIdleTime)
            {
                _timer?.Stop();
                await DisplayAlertAsync("Error", "Slideshow stopped due to repeated errors.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        _displaying = false;
    }
}
