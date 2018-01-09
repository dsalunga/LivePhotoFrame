using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    class FileSystemPhotoProvider
    {
        IReadOnlyList<StorageFile> files;
        var fileIndex = 0;

        public FileSystemPhotoProvider()
        {
        }

        public async void Init()
        {
            var basePath = @"D:\Pictures\LivePhotoFrame";
            var folder = await StorageFolder.GetFolderFromPathAsync(basePath + @"\Albums\Current\");
            files = await folder.GetFilesAsync();
            if (files.Count > 1)
            {
                async void displayImage()
                {
                    
                    await bitmapImage.SetSourceAsync(stream);
                    image.Source = bitmapImage;

                    fileIndex++;
                    if (fileIndex >= files.Count)
                    {
                        fileIndex = 0;
                    }
                }
                displayImage();

                var timer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 1, 0)
                };
                timer.Tick += (object sender, object args) =>
                {
                    displayImage();
                };
                timer.Start();
            }
        }

        public int Count
        {
            get
            {
                return files.Count;
            }
        }

        public async IRandomAccessStreamWithContentType NextStream()
        {
            if(files.Count > 0)
            {
                var file = files[fileIndex];
                //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
                var stream = await file.OpenReadAsync();
                return stream;
            }

            return null;
        }
    }
}
