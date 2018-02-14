using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
