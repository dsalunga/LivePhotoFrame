using LivePhotoFrame.Maui.Pages;
using LivePhotoFrame.Maui.Services;
using Microsoft.Extensions.Logging;

namespace LivePhotoFrame.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<AppConfigService>();

		// Pages
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<SlideshowPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
