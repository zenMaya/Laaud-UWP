using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Laaud_UWP.Util
{
    public static class FileUtil
    {
        public static async Task<int> GetFilesCountInAllDirectoriesAsync(StorageFolder rootFolder)
        {
            int filesCount = 0;

            // recursively search through all folders
            IReadOnlyList<StorageFolder> folders = await rootFolder.GetFoldersAsync(Windows.Storage.Search.CommonFolderQuery.DefaultQuery);
            foreach (StorageFolder subfolder in folders)
            {
                filesCount += await GetFilesCountInAllDirectoriesAsync(subfolder);
            }

            // add files count to the filesCount variable
            filesCount += (await rootFolder.GetFilesAsync()).Count;

            return filesCount;
        }
    }
}
