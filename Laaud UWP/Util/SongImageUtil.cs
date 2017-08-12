using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Laaud_UWP.Util
{
    public static class SongImageUtil
    {
        private static StorageFolder imagesFolderCached;

        public static async Task SaveImageAsync(int songId, byte[] imageData, string imageMimetype)
        {
            StorageFolder imagesFolder = await GetImagesFolder();
            StorageFile imageFile = await imagesFolder.CreateFileAsync(songId.ToString());
            await FileIO.WriteBytesAsync(imageFile, imageData);
        }

        public static async Task<StorageFile> LoadImageAsync(int songId)
        {
            StorageFolder imagesFolder = await GetImagesFolder();
            return (StorageFile)await imagesFolder.TryGetItemAsync(songId.ToString());
        }

        private static async Task<StorageFolder> GetImagesFolder()
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
