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

        // Keyboard input for desktop platforms
        RegisterKeyboardHandlers();
    }

    private void RegisterKeyboardHandlers()
    {
#if WINDOWS
        var nativeView = this.Handler?.PlatformView as Microsoft.UI.Xaml.UIElement;
        if (nativeView is not null)
        {
            nativeView.KeyDown += OnNativeKeyDown;
        }
#endif
    }

    private void UnregisterKeyboardHandlers()
    {
#if WINDOWS
        var nativeView = this.Handler?.PlatformView as Microsoft.UI.Xaml.UIElement;
        if (nativeView is not null)
        {
            nativeView.KeyDown -= OnNativeKeyDown;
        }
#endif
    }

#if WINDOWS
    private void OnNativeKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Right:
            case Windows.System.VirtualKey.Down:
            case Windows.System.VirtualKey.Space:
                DisplayPhoto(previous: false, restartTimer: true);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.Up:
                DisplayPhoto(previous: true, restartTimer: true);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Escape:
                Dispatcher.Dispatch(async () => await Shell.Current.GoToAsync(".."));
                e.Handled = true;
                break;
        }
    }
#endif

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _provider?.Dispose();
        UnregisterKeyboardHandlers();

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
            var displayed = false;
            var portraitSkipped = 0;

            while (!displayed && portraitSkipped < _provider.Count)
            {
                using var stream = await (previous
                    ? _provider.PreviousStreamAsync()
                    : _provider.NextStreamAsync());

                if (stream is null) break;

                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;

                var isPortrait = TryIsPortrait(memStream, out var portrait);
                if (isPortrait)
                {
                    isPortrait = portrait;
                }
                else
                {
                    isPortrait = false;
                }

                if (_config.SkipPortraits && isPortrait)
                {
                    portraitSkipped++;
                    continue;
                }

                var imageBytes = memStream.ToArray();
                photoImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                photoImage.Aspect = _config.ImageDisplayMode switch
                {
                    ImageDisplayMode.Uniform => Aspect.AspectFit,
                    ImageDisplayMode.UniformToFill => Aspect.AspectFill,
                    ImageDisplayMode.BestFit => isPortrait ? Aspect.AspectFit : Aspect.AspectFill,
                    _ => Aspect.AspectFit,
                };

                displayed = true;
            }

            if (!displayed && _config.SkipPortraits && portraitSkipped >= _provider.Count)
            {
                await DisplayAlertAsync("No photos to display", "All photos are portrait and 'Skip Portraits' is enabled.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

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

    private static bool TryIsPortrait(Stream stream, out bool isPortrait)
    {
        isPortrait = false;
        if (!stream.CanSeek) return false;

        try
        {
            stream.Position = 0;
            var info = SixLabors.ImageSharp.Image.Identify(stream);
            if (info is null) return false;

            isPortrait = info.Height > info.Width;
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream.Position = 0;
        }
    }
}
