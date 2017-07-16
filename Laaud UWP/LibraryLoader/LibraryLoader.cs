using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Laaud_UWP
{
    class LibraryLoader
    {
        private List<DirectoryLoader> musicDirectories = new List<DirectoryLoader>();

        public LibraryLoader()
        {

        }

        public void AddPath(StorageFolder folder)
        {
            this.musicDirectories.Add(new DirectoryLoader(folder));
        }

        public void AddPaths(IEnumerable<StorageFolder> folders)
        {
            this.musicDirectories.AddRange(folders.Select(stringPath => new DirectoryLoader(stringPath)));
        }

        public void ReloadAllPaths()
        {
            foreach (DirectoryLoader musicDirectory in this.musicDirectories)
            {
                musicDirectory.Read();
            }
        }
    }
}
