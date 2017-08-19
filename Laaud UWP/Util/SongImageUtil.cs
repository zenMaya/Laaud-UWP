using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Laaud_UWP.Util
{
    public static class SongImageUtil
    {
        private static StorageFolder imagesFolderCached;

        public static async Task SaveImageAsync(int songId, byte[] imageData, string imageMimetype)
        {
            StorageFolder imagesFolder = await GetImagesFolderAsync();
            StorageFile imageFile = await imagesFolder.CreateFileAsync(songId.ToString(), CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(imageFile, imageData);
        }

        public static async Task<StorageFile> LoadStorageFileAsync(int songId)
        {
            StorageFolder imagesFolder = await GetImagesFolderAsync();
            return (StorageFile)await imagesFolder.TryGetItemAsync(songId.ToString());
        }

        public static async Task<BitmapImage> LoadImageAsync(int songId)
        {
            StorageFile file = await LoadStorageFileAsync(songId);
            if (file == null)
            {
                return ImageUtil.GetAssetsBitmapImageByFileName("Favor.png");
            }
            else
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(await file.OpenAsync(FileAccessMode.Read));
                return image;
            }
        }

        private static async Task<StorageFolder> GetImagesFolderAsync()
        {
            if (imagesFolderCached == null)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder imagesFolder = (StorageFolder)await localFolder.TryGetItemAsync("Images");
                if (imagesFolder == null)
                {
                    imagesFolder = await localFolder.CreateFolderAsync("Images");
                }

                imagesFolderCached = imagesFolder;
            }

            return imagesFolderCached;
        }
    }
}
