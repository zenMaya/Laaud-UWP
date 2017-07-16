using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLibUWP;
using Windows.Storage;

namespace Laaud_UWP
{
    class DirectoryLoader
    {
        private static readonly HashSet<string> allowedExtensions = new HashSet<string> { ".MP3", ".WAV", ".OGG", ".FLAC", ".M4A", ".MP4" };

        public StorageFolder RootFolder { get; private set; }

        public DirectoryLoader(StorageFolder rootFolder)
        {
            this.RootFolder = rootFolder;
        }

        public void Read()
        {
            Action<StorageFolder> directorySearch = null;
            directorySearch = async (rootFolder) =>
            {
                // recursively search through all folders
                IReadOnlyList<StorageFolder> folders = await rootFolder.GetFoldersAsync(Windows.Storage.Search.CommonFolderQuery.DefaultQuery);
                foreach (StorageFolder folder in folders)
                {
                    directorySearch(folder);
                }

                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    // get all files in directory
                    foreach (StorageFile file in await rootFolder.GetFilesAsync())
                    {
                        // continue for sound files only
                        if (allowedExtensions.Contains(file.FileType.ToUpperInvariant()))
                        {
                            try
                            {
                                // try searching for an existing song in the DB by the path
                                Song existingSong = dbContext.Songs.FirstOrDefault(song => song.Path == file.Path);
                                if (existingSong == null)
                                {
                                    // if not found, create a new one
                                    Song newSong = new Song();
                                    newSong.ReadFromTag(await Task.Run(() => TagManager.ReadFile(file).Tag), dbContext);
                                    dbContext.Songs.Add(newSong);
                                }
                                else
                                {
                                    // if found, only update its info
                                    existingSong.ReadFromTag(TagManager.ReadFile(file).Tag, dbContext);
                                }

                                // save create/update action to DB
                                dbContext.SaveChanges();
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
            };

            directorySearch(this.RootFolder);
        }
    }
}
