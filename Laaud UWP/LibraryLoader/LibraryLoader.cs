using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Laaud_UWP.LibraryLoader
{
    class LibraryLoader
    {
        private List<DirectoryLoader> musicDirectories = new List<DirectoryLoader>();

        public event EventHandler<LibraryLoadProgressUpdateArgs> ProgressUpdated;

        public LibraryLoader()
        {

        }

        public void AddPath(StorageFolder folder)
        {
            DirectoryLoader directoryLoader = new DirectoryLoader(folder);
            directoryLoader.ProgressUpdated += ProgressUpdated;
            this.musicDirectories.Add(directoryLoader);
        }

        public void AddPaths(IEnumerable<StorageFolder> folders)
        {
            this.musicDirectories.AddRange(folders.Select(stringPath =>
            {
                DirectoryLoader directoryLoader = new DirectoryLoader(stringPath);
                directoryLoader.ProgressUpdated += ProgressUpdated;
                return directoryLoader;
            }));
        }

        public async Task ReloadAllPathsAsync()
        {
            foreach (DirectoryLoader musicDirectory in this.musicDirectories)
            {
                await musicDirectory.ReadAsync();
            }
        }
    }
}
