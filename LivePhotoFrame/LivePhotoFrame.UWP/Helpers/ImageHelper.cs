using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LivePhotoFrame.UWP.Helpers
{
    public class ImageHelper
    {
        public static bool IsImageInOppositeOrientation(FrameworkElement element, Image image)
        {
            return false;
        }

        public static void SetImageScale(FrameworkElement element, Image image)
        {

        }

        public static bool IsPortrait(BitmapImage image)
        {
            return image.PixelHeight > image.PixelWidth;
        }
    }
}
