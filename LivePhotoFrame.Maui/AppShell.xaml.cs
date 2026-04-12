using LivePhotoFrame.Maui.Pages;

namespace LivePhotoFrame.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(SlideshowPage), typeof(SlideshowPage));
	}
}
