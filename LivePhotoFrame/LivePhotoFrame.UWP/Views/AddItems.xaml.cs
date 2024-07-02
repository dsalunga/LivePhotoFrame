using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using LivePhotoFrame.Models;
using LivePhotoFrame.ViewModels;

namespace LivePhotoFrame.UWP.Views
{
	public sealed partial class AddItems : Page
	{
		ItemsViewModel ViewModel;
		public AddItems()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel = (ItemsViewModel)e.Parameter;
			DataContext = ViewModel;
		}

		private void SaveItem_Click(object sender, RoutedEventArgs e)
		{
			var item = new Item
			{
				Text = txtText.Text,
				Description = txtDesc.Text
			};
			ViewModel.AddItemCommand.Execute(item);

			this.Frame.GoBack();
		}
	}
}