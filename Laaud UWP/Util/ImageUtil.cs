using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Laaud_UWP.Util
{
    class ImageUtil
    {
        public static Uri GetAssetsImageUriByFileName(string fileName)
        {
            return new Uri("ms-appx:///Assets/" + fileName, UriKind.Absolute);
        }

        public static BitmapImage GetAssetsBitmapImageByFileName(string fileName)
        {
            return new BitmapImage(GetAssetsImageUriByFileName(fileName));
        }
    }
}
