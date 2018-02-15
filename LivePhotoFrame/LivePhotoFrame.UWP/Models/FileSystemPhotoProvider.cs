using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

using LivePhotoFrame.Helpers;

namespace LivePhotoFrame.UWP.Models
{
    class FileSystemPhotoProvider : IPhotoProvider
    {
        public const string TAG = "FileSystem";

        List<StorageFile> files;
        int fileIndex = 0;

        public int Count => files != null ? files.Count : 0;

        public string CurrentFileName => files != null && files.Count > 0 ? files[fileIndex].Name : string.Empty;

        public FileSystemPhotoProvider()
        {
        }

        public async Task Initialize()
        {
            var config = AppConfigManager.GetInstance().GetConfig();

            // works with $> mklink /j junctiondirname junctiontargetdir
            var folder = await StorageFolder.GetFolderFromPathAsync(config.FileSystemPath);
            var storageFiles = await folder.GetFilesAsync();
            files = new List<StorageFile>(storageFiles);
            files.Shuffle();
        }

        public async Task<IRandomAccessStream> NextStream()
        {
            if(files.Count > 0)
            {
                fileIndex++;
                if (fileIndex >= files.Count)
                    fileIndex = 0;

                //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
                IRandomAccessStreamWithContentType stream = await ReadStream();
                return stream;
            }

            return null;
        }

        private async Task<IRandomAccessStreamWithContentType> ReadStream()
        {
            var file = files[fileIndex];
            var stream = await file.OpenReadAsync();
            return stream;
        }

        public async Task<IRandomAccessStream> PreviousStream()
        {
            if (files.Count > 0)
            {
                fileIndex--;
                if (fileIndex == -1)
                    fileIndex = files.Count - 1;

                //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
                IRandomAccessStreamWithContentType stream = await ReadStream();
                return stream;
            }

            return null;
        }

        public void Done()
        {

        }
    }
}
