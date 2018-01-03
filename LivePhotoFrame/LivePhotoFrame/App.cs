using System;
using LivePhotoFrame.Helpers;
using LivePhotoFrame.Models;
using LivePhotoFrame.Services;

namespace LivePhotoFrame
{
    public class App
    {

        public static void Initialize()
        {
            ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
        }
    }
}
