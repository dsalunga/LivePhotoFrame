using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using LivePhotoFrame.Models;
using LivePhotoFrame.ViewModels;

namespace LivePhotoFrame.UWP.Views
{
	public sealed partial class MainPivot : Page
	{
		public ItemsViewModel BrowseViewModel { get; private set; }
		public AboutViewModel AboutViewModel { get; private set; }

		public MainPivot()
		{
			NavigationCacheMode = NavigationCacheMode.Required;

			InitializeComponent();

			BrowseViewModel = new ItemsViewModel();
			AboutViewModel = new AboutViewModel();

			BrowseViewModel.LoadItemsCommand.Execute(null);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (BrowseViewModel.Items.Count == 0)
				BrowseViewModel.LoadItemsCommand.Execute(null);
		}

		public void AddItem_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(AddItems), BrowseViewModel);
		}

		private void GvItems_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = e.ClickedItem as Item;
			
            switch (item.Id)
            {
                case "Launch":
                    //await Navigation.PushModalAsync(new NavigationPage(new LivePhotoFrame()));
                    this.Frame.Navigate(typeof(LivePhotoFrame), item);
                    break;
                case "Settings":
                    this.Frame.Navigate(typeof(Settings));
                    break;
                default:
                    this.Frame.Navigate(typeof(BrowseItemDetail), item);
                    break;
            }
        }

		private void PivotItemChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (((Pivot)sender).SelectedIndex)
			{
				case 0:
					Toolbar.Visibility = Visibility.Visible;
					break;
				case 1:
					Toolbar.Visibility = Visibility.Collapsed;
					break;
			}
		}
	}
}