using LivePhotoFrame.UWP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LivePhotoFrame.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LivePhotoFrame : Page
    {
        IPhotoProvider provider;
        DispatcherTimer timer;

        public LivePhotoFrame()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        private void GoBack()
        {
            // Detatch from key inputs event
            //Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
            Window.Current.CoreWindow.KeyUp -= CoreWindow_KeyUp;

            timer.Stop();
            if(provider != null)
            {
                provider.Done();
            }
            this.Frame.GoBack();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }

            Window.Current.CoreWindow.PointerCursor = null;

            this.DoubleTapped += LivePhotoFrame_DoubleTapped;
            //this.Tapped += LivePhotoFrame_Tapped;

            // Attach to key inputs event
            //Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            this.ManipulationDelta += LivePhotoFrame_ManipulationDelta;
            this.ManipulationCompleted += LivePhotoFrame_ManipulationCompleted;
            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateInertia | ManipulationModes.System;

            //bitmapImage.UriSource = new Uri(installedLocation.Path + @"\Assets\pigs.jpg");

            /*var myPictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            IObservableVector<StorageFolder> myPictureFolders = myPictures.Folders;
            foreach(var folder in myPictureFolders)
            {
                Debug.WriteLine(folder.Path);
            }*/

            /*StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var files = await storageFolder.GetFilesAsync();
            var file = files.FirstOrDefault();*/

            //https://xamarin.com/content/images/pages/forms/example-app.png
            //ms-appx:///Assets/pigs.jpg
            //  same as
            //  installedLocation.Path + @"\Assets\pigs.jpg"
            //storageFolder.Path + @"\pigs.jpg"

            // Photos from Picture Library are not accessible via UriSource, it has to be via Stream.
            //var file = await StorageFile.GetFileFromPathAsync(myPictures.SaveFolder.Path + @"\LivePhotoFrame\Others\pigs.jpg");

            var config = AppConfigManager.GetInstance().GetConfig();
            switch (config.ActiveSource)
            {
                case FtpPhotoProvider.TAG:
                    provider = new FtpPhotoProvider();
                    break;

                case FileSystemPhotoProvider.TAG:
                    provider = new FileSystemPhotoProvider();
                    break;
            }
            //provider = new FtpPhotoProvider();
            await provider.Initialize();
            if (provider.Count > 0)
            {
                DisplayImage();

                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, config.Interval, 0);
                timer.Tick += (object sender, object args) =>
                {
                    DisplayImage();
                };
                timer.Start();
            }


            /*FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Open a stream for the selected file 
            StorageFile file = await openPicker.PickSingleFileAsync(); //StorageFile.GetFileFromPathAsync(@"localdiskpath"); //
            //StorageFile file = await KnownFolders.PicturesLibrary.GetFileAsync(@"localdiskpath");

            // Ensure a file was selected 
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap 
                    //bitmapImage.DecodePixelWidth = 600; //match the target Image.Width, not shown
                    await bitmapImage.SetSourceAsync(fileStream);
                    image.Source = bitmapImage;
                }
            }*/
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            switch(args.VirtualKey)
            {
                case VirtualKey.Escape:
                    GoBack();
                    break;

                case VirtualKey.Left:
                case VirtualKey.Right:
                    DisplayImage();
                    break;
            }
        }

        private bool isSwiped;

        private void LivePhotoFrame_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            isSwiped = false;
        }

        private void LivePhotoFrame_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !isSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;
                if (Math.Abs(swipedDistance) <= 2) return;
                if (swipedDistance > 0)
                {
                    // Right
                    DisplayImage();
                }
                else
                {
                    // Left
                    DisplayImage();
                }
                isSwiped = true;
            }
        }

        private async void DisplayImage()
        {
            //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
            var stream = await provider.NextStream();
            if (stream != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                image.Source = bitmapImage;
            }
        }

        private void LivePhotoFrame_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            GoBack();
        }

        /*
        private void LivePhotoFrame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayImage();
        }*/



        // Refer to https://github.com/mixer/uwp-keycodes/blob/master/index.ts
        /*class Keys
        {
            public const uint Escape = 27;
            public const uint LeftArrow = 37;
            public const uint RightArrow = 39;
        }
        
        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            switch(args.KeyCode)
            {
                case Keys.Escape:
                    GoBack();
                    break;

                case Keys.LeftArrow:
                case Keys.RightArrow:
                    DisplayImage();
                    break;
            }
        }*/
    }
}
