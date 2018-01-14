using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public async Task Init()
        {
            client = new FtpClient("ftphostname", "username", "password");
            client.Connect();

            items = await client.GetListingAsync("/Albums/Current/");
        }

        public async Task<IRandomAccessStream> NextStream()
        {
            if (items.Length > 0)
            {
                var file = items[fileIndex];

                Stream stream = await client.OpenReadAsync(file.FullName);

                // Create a .NET memory stream.
                var memStream = new MemoryStream();

                // Convert the stream to the memory stream, because a memory stream supports seeking.
                await stream.CopyToAsync(memStream);

                // Set the start position.
                memStream.Position = 0;

                fileIndex++;
                if (fileIndex >= items.Length)
                {
                    fileIndex = 0;
                }

                return memStream.AsRandomAccessStream();
            }

            return null;
        }

        public void Done()
        {
            // disconnect! good bye!
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
            }
        }
    }
}
