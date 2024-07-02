using LivePhotoFrame.UWP.Helpers;
using LivePhotoFrame.UWP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LivePhotoFrame.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LivePhotoFrame : Page
    {
        DisplayRequest displayRequest;
        IPhotoProvider provider;
        DispatcherTimer timer;
        AppConfig config;
        int totalIdleTime = 0; // in minutes

        public LivePhotoFrame()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        private void GoBack()
        {
            // Detatch from key inputs event
            //Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
            Window.Current.CoreWindow.KeyUp -= CoreWindow_KeyUp;

            if (timer != null)
                timer.Stop();
            if (provider != null)
                provider.Done();
            if(displayRequest != null)
                displayRequest.RequestRelease();

            Frame.GoBack();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // TODO Windows.UI.ViewManagement.ApplicationView is no longer supported. Use Microsoft.UI.Windowing.AppWindow instead. For more details see https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/windowing
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

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

            config = AppConfigManager.GetInstance().GetConfig();
            switch (config.ActiveSource)
            {
                case FtpPhotoProvider.TAG:
                    provider = new FtpPhotoProvider();
                    break;

                case FileSystemPhotoProvider.TAG:
                    provider = new FileSystemPhotoProvider();
                    break;
            }

            try
            {
                await provider.Initialize();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(ex.Message);
                return;
            }

            if(provider.Count == 1)
            {
                await ShowErrorDialog("Only one image is found. This is not recommended as it may cause monitor ghosting or burn-in.");
                return;
            }

            if (provider.Count > 0)
            {
                switch (config.ImageDisplayMode)
                {
                    case ImageDisplayMode.Uniform:
                        image.Stretch = Stretch.Uniform;
                        break;

                    case ImageDisplayMode.UniformToFill:
                        image.Stretch = Stretch.UniformToFill;
                        break;
                }

                DisplayImage();

                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, config.Interval, 0);
                timer.Tick += (object sender, object args) => DisplayImage();
                timer.Start();

                displayRequest = new DisplayRequest();
                displayRequest.RequestActive();
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

        private async Task ShowErrorDialog(String errorMessage)
        {
            var messageDialog = new MessageDialog(errorMessage);
            messageDialog.Commands.Add(new UICommand("OK", (command) => GoBack()));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 0;
            await messageDialog.ShowAsync();
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Escape:
                    GoBack();
                    break;

                case VirtualKey.Left:
                    DisplayImage(true, true);
                    break;

                case VirtualKey.Right:
                    DisplayImage(false, true);
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
                    // Left to Right
                    DisplayImage(true, true);
                }
                else
                {
                    // Right to Left
                    DisplayImage(false, true);
                }
                isSwiped = true;
            }
        }

        private bool displaying;

        private async void DisplayImage(bool previous = false, bool restartTimer = false)
        {
            //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
            if (displaying) return;
            displaying = true;

            try
            {
                int portraitSkipped = 0;
                do
                {
                    var stream = await (previous ? provider.PreviousStream() : provider.NextStream());
                    if (stream != null)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);

                        if (config.SkipPortraits && ImageHelper.IsPortrait(bitmapImage))
                        {
                            Debug.WriteLine("Portrait skipped: " + provider.CurrentFileName);
                            portraitSkipped++;
                        }
                        else
                        {
                            if (config.ImageDisplayMode == ImageDisplayMode.BestFit)
                            {
                                image.Stretch = ImageHelper.IsPortrait(bitmapImage) ? Stretch.Uniform : Stretch.UniformToFill;
                            }
                            image.Source = bitmapImage;
                            portraitSkipped = 0;
                        }
                    }
                } while (portraitSkipped > 0 && portraitSkipped != provider.Count); // while aPhotoIsSkipped and notAllPhotosArePortrait

                totalIdleTime = 0;
                if (restartTimer)
                    timer.Start();
            }
            catch (Exception e)
            {
                totalIdleTime += config.Interval;
                if (totalIdleTime >= config.MaxIdleTime)
                {
                    timer.Stop();
                    displayRequest.RequestRelease();
                    await ShowErrorDialog(e.Message);
                }
            }

            displaying = false;
        }

        private DateTime lastDoubleTapped = DateTime.MinValue;

        private void LivePhotoFrame_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // Must double-tap within 3 seconds
            var now = DateTime.Now;
            if (now.Subtract(lastDoubleTapped).TotalSeconds < 3)
            {
                lastDoubleTapped = DateTime.MinValue;
                GoBack();
            } else
            {
                lastDoubleTapped = now;
            }
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
