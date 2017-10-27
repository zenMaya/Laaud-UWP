using Id3;
using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            int progressStep = totalFileCount / 1000;
            int filesAlreadyProcessed = 0;
            int filesAlreadyProcessedTillLastProgressUpdate = progressStep + 1; // set so that we get status update as soon as we start updating the DB

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
                            Song song;

                            Mp3Stream tagStream = new Id3.Mp3Stream(await file.OpenStreamForReadAsync());
                            bool anySongTags;
                            Id3Tag[] songTags = null;
                            try
                            {
                                songTags = tagStream.GetAllTags();
                                anySongTags = songTags.Length > 0;
                            }
                            catch (Exception e)
                            {
                                anySongTags = false;
                            }

                            if (anySongTags)
                            {
                                Id3Tag songTag = songTags[0];

                                // search for an existing artist by name
                                Artist artist = Artist.CreateOrFind(dbContext, songTag.Artists.Value);

                                // search for an existing album by name and artist
                                Album album = Album.CreateOrFind(dbContext, songTag.Album, artist.ArtistId);

                                // search for an existing song by path
                                song = Song.CreateOrFind(dbContext, file.Path);

                                // set reference to the album
                                song.Album = album;

                                // load other simpler properties
                                if (songTag.Year.IsAssigned)
                                {
                                    song.Year = songTag.Year.AsDateTime.Value.Year;
                                }

                                if (songTag.Track.IsAssigned)
                                {
                                    song.Track = songTag.Track.AsInt.Value;
                                }

                                song.Title = songTag.Title;
                                song.Genre = songTag.Genre;
                                song.Comment = string.Join(", ", songTag.Comments);

                                // insert/update to DB
                                dbContext.SaveChanges();

                                // save image to localappdata
                                if (songTag.Pictures.Any())
                                {
                                    await SongImageUtil.SaveImageAsync(song.SongId, songTag.Pictures.First().PictureData, songTag.Pictures.First().MimeType);
                                }
                            }
                            else
                            {
                                song = Song.CreateOrFind(dbContext, file.Path);

                                song.Title = Path.GetFileNameWithoutExtension(file.Path);

                                // insert/update to DB
                                dbContext.SaveChanges();
                            }

                            // update progress
                            if (filesAlreadyProcessedTillLastProgressUpdate > progressStep)
                            {
                                filesAlreadyProcessedTillLastProgressUpdate = 0;
                                this.RaiseProgressUpdate((float)filesAlreadyProcessed / totalFileCount, song);
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
