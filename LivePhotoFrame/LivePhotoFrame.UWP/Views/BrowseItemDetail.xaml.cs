using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using LivePhotoFrame.Models;
using LivePhotoFrame.ViewModels;

namespace LivePhotoFrame.UWP.Views
{
	public sealed partial class BrowseItemDetail : Page
	{
		public ItemDetailViewModel ViewModel { get; set; }

		public BrowseItemDetail()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			ViewModel = new ItemDetailViewModel((Item)e.Parameter);
			DataContext = ViewModel;
		}
	}
}