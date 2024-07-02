using FluentFTP;
using LivePhotoFrame.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    class FtpPhotoProvider : IPhotoProvider
    {
        FtpClient client;
        FtpListItem[] items;
        int fileIndex = 0;

        public int Count => items != null ? items.Length : 0;

        public string CurrentFileName => items != null && items.Length > 0 ? items[fileIndex].Name : string.Empty;

        public const string TAG = "FTP";

        public async Task Initialize()
        {
            var config = AppConfigManager.GetInstance().GetConfig();

            client = new FtpClient(config.FtpConfig.Hostname, config.FtpConfig.Username, config.FtpConfig.Password);
            client.Connect();

            items = client.GetListing(config.FtpConfig.Path);
            items.Shuffle();

            await PhotoCacheManager.GetInstance().Prepare();
        }

        public async Task<IRandomAccessStream> NextStream()
        {
            if (items.Length > 0)
            {
                fileIndex++;
                if (fileIndex >= items.Length)
                    fileIndex = 0;

                return await ReadStream();
            }

            return null;
        }

        public async Task<IRandomAccessStream> PreviousStream()
        {
            if (items.Length > 0)
            {
                fileIndex--;
                if (fileIndex == -1)
                    fileIndex = items.Length - 1;

                return await ReadStream();
            }

            return null;
        }

        private async Task<IRandomAccessStream> ReadStream()
        {
            var file = items[fileIndex];
            // Check cache
            var cache = await PhotoCacheManager.GetInstance().Read(file.Name);
            if (cache != null)
                return cache;

            if (!client.IsConnected)
                client.Connect();

            Stream stream = client.OpenRead(file.FullName);

            // Convert the stream to the memory stream, because a memory stream supports seeking.
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);

            // Save to cache
            await PhotoCacheManager.GetInstance().Write(file.Name, memStream);

            // Set the start position.
            memStream.Position = 0;

            client.Disconnect();
            return memStream.AsRandomAccessStream();
        }

        public void Done()
        {
            // disconnect! good bye!
            if (client != null && client.IsConnected)
                client.Disconnect();
        }
    }
}
