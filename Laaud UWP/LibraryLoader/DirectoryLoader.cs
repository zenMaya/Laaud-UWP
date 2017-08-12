using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLibUWP;
using Windows.Storage;

namespace Laaud_UWP.LibraryLoader
{
    class DirectoryLoader
    {
        private static readonly HashSet<string> allowedExtensions = new HashSet<string> { ".MP3", ".WAV", ".OGG", ".FLAC", ".M4A", ".MP4" };

        public StorageFolder RootFolder { get; private set; }

        public event EventHandler<LibraryLoadProgressUpdateArgs> ProgressUpdated;

        public DirectoryLoader(StorageFolder rootFolder)
        {
            this.RootFolder = rootFolder;
        }

        public async Task ReadAsync()
        {
            this.RaiseProgressUpdate(0, null);

            int totalFileCount = await FileUtil.GetFilesCountInAllDirectoriesAsync(this.RootFolder);
            int progressStep = totalFileCount / 100;
            int filesAlreadyProcessed = 0;
            int filesAlreadyProcessedTillLastProgressUpdate = 0;

            async Task directorySearch(StorageFolder rootFolder)
            {
                // recursively search through all folders
                IReadOnlyList<StorageFolder> folders = await rootFolder.GetFoldersAsync(Windows.Storage.Search.CommonFolderQuery.DefaultQuery);
                foreach (StorageFolder folder in folders)
                {
                    await directorySearch(folder);
                }

                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    // get all files in directory
                    foreach (StorageFile file in await rootFolder.GetFilesAsync())
                    {
                        filesAlreadyProcessed++;
                        filesAlreadyProcessedTillLastProgressUpdate++;

                        // continue for sound files only
                        if (allowedExtensions.Contains(file.FileType.ToUpperInvariant()))
                        {
                            try
                            {
                                // try searching for an existing song in the DB by the path
                                Tag songTag = await Task.Run(() => TagManager.ReadFile(file).Tag);

                                // search for an existing artist by name
                                Artist artist = dbContext.Artists.FirstOrDefault(_artist => _artist.Name == songTag.Artist);
                                if (artist == null)
                                {
                                    // if not found, create a new one
                                    artist = new Artist()
                                    {
                                        Name = songTag.Artist
                                    };

                                    dbContext.Artists.Add(artist);
                                    dbContext.SaveChanges();
                                }

                                // search for an existing album by name
                                Album album = dbContext.Albums.FirstOrDefault(_album => _album.Name == songTag.Album);
                                if (album == null)
                                {
                                    // if not found, create a new one
                                    album = new Album()
                                    {
                                        Name = songTag.Album,
                                        Artist = artist
                                    };

                                    dbContext.Albums.Add(album);
                                    dbContext.SaveChanges();
                                }

                                Song song = dbContext.Songs.FirstOrDefault(_song => _song.Path == file.Path);
                                if (song == null)
                                {
                                    // if not found, create a new one
                                    song = new Song()
                                    {
                                        Path = file.Path
                                    };

                                    dbContext.Songs.Add(song);
                                }

                                // set reference to the album
                                song.Album = album;

                                // load other simpler properties
                                song.Year = (int)songTag.Year;
                                song.Track = (int)songTag.Track;
                                song.Title = songTag.Title;
                                song.Genre = songTag.Genre;
                                song.Comment = songTag.Comment;

                                // insert/update to DB
                                dbContext.SaveChanges();

                                // save image to localappdata
                                if (songTag.Image != null)
                                {
                                    await SongImageUtil.SaveImageAsync(song.SongId, songTag.Image.Data, songTag.Image.MIMEType);
                                }

                                // update progress
                                if (filesAlreadyProcessedTillLastProgressUpdate > progressStep)
                                {
                                    filesAlreadyProcessedTillLastProgressUpdate = 0;
                                    this.RaiseProgressUpdate((float)filesAlreadyProcessed / totalFileCount, song);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
            };

            if (totalFileCount > 0)
            {
                await directorySearch(this.RootFolder);
            }
        }

        private void RaiseProgressUpdate(float progress, Song lastProcessedSong)
        {
            this.ProgressUpdated?.Invoke(this, new LibraryLoadProgressUpdateArgs(progress, lastProcessedSong));
        }
    }
}
