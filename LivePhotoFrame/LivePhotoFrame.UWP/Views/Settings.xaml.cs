using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using LivePhotoFrame.Models;
using LivePhotoFrame.ViewModels;
using LivePhotoFrame.UWP.Models;
using LivePhotoFrame.UWP.Helpers;

namespace LivePhotoFrame.UWP.Views
{
	public sealed partial class Settings : Page
	{
        private AppConfig config;

        //AppConfig ViewModel;
        public Settings()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

            config = AppConfigManager.GetInstance().GetConfig();

            txtFtpHostname.Text = config.FtpConfig.Hostname;
            txtFtpPath.Text = config.FtpConfig.Path;
            txtUsername.Text = config.FtpConfig.Username;
            txtPassword.Text = config.FtpConfig.Password;
            txtFileSystemPath.Text = config.FileSystemPath;
            txtInterval.Text = config.Interval.ToString();
            txtMaxIdle.Text = config.MaxIdleTime.ToString();
            checkboxAutoStartShow.IsChecked = config.AutoStartShow;

            switch(config.ActiveSource)
            {
                case FtpPhotoProvider.TAG:
                    radioFtp.IsChecked = true;
                    break;

                case FileSystemPhotoProvider.TAG:
                    radioFileSystem.IsChecked = true;
                    break;
            }
		}

		private void SaveItem_Click(object sender, RoutedEventArgs e)
		{
            config.FileSystemPath = txtFileSystemPath.Text.Trim();
            config.Interval = int.Parse(txtInterval.Text.Trim());
            config.MaxIdleTime = int.Parse(txtMaxIdle.Text.Trim());
            config.AutoStartShow = checkboxAutoStartShow.IsChecked.Value;

            config.FtpConfig.Hostname = txtFtpHostname.Text.Trim();
            config.FtpConfig.Path = txtFtpPath.Text.Trim();
            config.FtpConfig.Username = txtUsername.Text.Trim();
            config.FtpConfig.Password = txtPassword.Text.Trim();

            config.Save();

            this.Frame.GoBack();
		}

        private void ActiveSourceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)e.OriginalSource;
            var tag = radioButton.Tag.ToString();
            config.ActiveSource = tag;
        }
    }
}