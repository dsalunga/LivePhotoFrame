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

        public int Count
        {
            get
            {
                return files.Count;
            }
        }

        public async Task<IRandomAccessStream> NextStream()
        {
            if(files.Count > 0)
            {
                var file = files[fileIndex];
                var stream = await file.OpenReadAsync();

                fileIndex++;
                if (fileIndex >= files.Count)
                    fileIndex = 0;
                //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
                
                return stream;
            }

            return null;
        }

        public void Done()
        {

        }
    }
}
