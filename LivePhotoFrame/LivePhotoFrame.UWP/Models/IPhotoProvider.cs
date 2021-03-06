﻿using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace LivePhotoFrame.UWP.Models
{
    interface IPhotoProvider
    {
        int Count { get; }

        Task Initialize();
        Task<IRandomAccessStream> NextStream();

        Task<IRandomAccessStream> PreviousStream();

        string CurrentFileName { get; }

        void Done();
    }
}