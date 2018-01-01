using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
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

            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            await Navigation.PopModalAsync();
        }
    }
}