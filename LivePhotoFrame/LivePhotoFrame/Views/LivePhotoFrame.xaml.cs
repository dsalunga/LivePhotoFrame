using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if __WINDOWS_UWP__
using Windows.UI.ViewManagement;
#endif
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LivePhotoFrame.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LivePhotoFrame : ContentPage
	{
		public LivePhotoFrame ()
		{
			InitializeComponent ();

#if __WINDOWS_UWP__
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
#endif
        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
#if __WINDOWS_UWP__
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
#endif
            await Navigation.PopModalAsync();
        }
    }
}