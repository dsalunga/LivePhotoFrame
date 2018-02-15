using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    public class PhotoCacheManager
    {
        private static PhotoCacheManager instance;

        public static PhotoCacheManager GetInstance()
        {
            if(instance == null)
                instance = new PhotoCacheManager();

            return instance;
        }

        private StorageFolder cacheFolder;
        private Dictionary<string, StorageFile> files;
        private bool initialized;

        public async Task Prepare()
        {
            if (initialized)
                return;

            AppConfig config = AppConfigManager.GetInstance().GetConfig();

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            cacheFolder = await storageFolder.CreateFolderAsync(@"Albums\Cache", CreationCollisionOption.OpenIfExists);

            files = (await cacheFolder.GetFilesAsync()).ToDictionary(x => x.Name, y => y);

            /*var files = await storageFolder.GetFilesAsync();
            var file = files.FirstOrDefault();*/

            initialized = true;
        }

        public string GetCacheFolder()
        {
            return cacheFolder.Path;
        }

        const double gigabyte = 1024 * 1024 * 1024;

        public async Task<Tuple<string, ulong>> GetCacheSize()
        {
            ulong totalSize = 0;
            foreach(var file in files)
            {
                var basicProperties = await file.Value.GetBasicPropertiesAsync();
                totalSize += basicProperties.Size;
            }

            string size = (totalSize / gigabyte).ToString("0.00") + " GB";
            return new Tuple<string, ulong>(size, totalSize);

            /*if(properties.Size > (1024*1024*1024))
            {
                var files = await cacheFolder.GetFilesAsync();
                foreach(var file in files)
                {
                    await file.DeleteAsync();
                }
            }*/
        }

        public async Task<IRandomAccessStream> Read(string name)
        {
            if (files.ContainsKey(name))
            {
                var file = await cacheFolder.TryGetItemAsync(name);
                if (file != null)
                {
                    var stream = await ((StorageFile)file).OpenReadAsync();
                    return stream;
                }
            }
            return null;
        }

        public async Task Write(string name, MemoryStream stream)
        {
            var file = await cacheFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            using (Stream outputStream = await file.OpenStreamForWriteAsync())
            {
                stream.Position = 0;
                await stream.CopyToAsync(outputStream);
            }

            files.Add(file.Name, file);
        }
    }
}
