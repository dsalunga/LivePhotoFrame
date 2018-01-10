using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    class FileSystemPhotoProvider
    {
        IReadOnlyList<StorageFile> files;
        int fileIndex = 0;

        public FileSystemPhotoProvider()
        {
        }

        public async Task Init()
        {
            var basePath = @"D:\Pictures\LivePhotoFrame";
            var folder = await StorageFolder.GetFolderFromPathAsync(basePath + @"\Albums\Current\");
            files = await folder.GetFilesAsync();
        }

        public int Count
        {
            get
            {
                return files.Count;
            }
        }

        public async Task<IRandomAccessStreamWithContentType> NextStream()
        {
            if(files.Count > 0)
            {
                var file = files[fileIndex];
                var stream = await file.OpenReadAsync();

                fileIndex++;
                if (fileIndex >= files.Count)
                {
                    fileIndex = 0;
                }
                //var file = await StorageFile.GetFileFromPathAsync(@"D:\Pictures\LivePhotoFrame\Others\pigs.jpg");
                
                return stream;
            }

            return null;
        }
    }
}
