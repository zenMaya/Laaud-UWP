using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laaud_UWP.Models;
using System.Threading;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Laaud_UWP.Util;

namespace Laaud_UWP
{
    class SongDBSearcher
    {
        private CancellationTokenSource loadingSongsCancellationTokenSource = null;
        private List<int> DBIDs;
        private GroupType _GroupType;

        public enum GroupType { song, album, artist };
        public event Action<Song> SongRecievedFromDBEvent;
        public event Action<Album> AlbumRecievedFromDBEvent;
        public event Action<Artist> ArtistRecievedFromDBEvent;
        

        public void Search(GroupType _GroupType, string[] phrases = null)
        {
            if (this.loadingSongsCancellationTokenSource != null)
            {
                this.loadingSongsCancellationTokenSource.Cancel();
            }

            Task.Factory.StartNew(() =>
            {
                this._GroupType = _GroupType;
                this.DBIDs.Clear();

                this.loadingSongsCancellationTokenSource = new CancellationTokenSource();

                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var phrase = phrases[0] == null ? phrases[0] : string.Empty;

                    switch (this._GroupType)
                    {
                        case GroupType.song:

                            DBIDs = dbContext.Songs
                                .Include(song => song.Album)
                                .ThenInclude(album => album.Artist)
                                .Where(
                                    song => song.Title.ContainsIgnoreCase(phrase)
                                    || song.Album.Name.ContainsIgnoreCase(phrase)
                                    || song.Album.Artist.Name.ContainsIgnoreCase(phrase))
                                .OrderBy(song => song.Title)
                                .Select(song => song.SongId)
                                .ToList();

                            stopwatch.Stop();
                            Debug.WriteLine(stopwatch.ElapsedMilliseconds);

                            break;



                        case GroupType.album:

                            DBIDs = dbContext.Songs
                                .Include(song => song.Album)
                                .ThenInclude(album => album.Artist)
                                .Where(
                                    song => song.Title.ContainsIgnoreCase(phrase)
                                    || song.Album.Name.ContainsIgnoreCase(phrase)
                                    || song.Album.Artist.Name.ContainsIgnoreCase(phrase))
                                .OrderBy(song => song.Album.Name)
                                .ThenBy(song => song.Track)
                                .Select(song => song.AlbumId)
                                .ToList();

                            stopwatch.Stop();
                            Debug.WriteLine(stopwatch.ElapsedMilliseconds);

                            break;

                        default: //artist
                            DBIDs = dbContext.Songs
                                .Include(song => song.Album)
                                .ThenInclude(album => album.Artist)
                                .Where(
                                    song => song.Title.ContainsIgnoreCase(phrase)
                                    || song.Album.Name.ContainsIgnoreCase(phrase)
                                    || song.Album.Artist.Name.ContainsIgnoreCase(phrase))
                                .OrderBy(song => song.Album.Artist.Name)
                                .ThenBy(song => song.Album.Name)
                                .ThenBy(song => song.Track)
                                .Select(song => song.Album.ArtistId)
                                .ToList();

                            stopwatch.Stop();
                            Debug.WriteLine(stopwatch.ElapsedMilliseconds);
                            break;
                    }

                    GetSearchResults(0, 30);
                }
            });
        }

        public void GetSearchResults(int firstElement, int range)
        {
            var maxVal = range + firstElement >= DBIDs.Count ? DBIDs.Count - 1 : range + firstElement; 
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                switch (this._GroupType)
                {
                    case GroupType.song:
                        for (int i = firstElement; i <= maxVal; i++)
                        {
                            this.SongRecievedFromDBEvent?.Invoke(dbContext.Songs.Find(DBIDs[i]));
                        }
                        break;

                    case GroupType.album:
                        for (int i = firstElement; i <= maxVal; i++)
                        {
                            this.AlbumRecievedFromDBEvent?.Invoke(dbContext.Albums.Find(DBIDs[i]));
                        }
                        break;

                    default:
                        for (int i = firstElement; i <= maxVal; i++)
                        {
                            this.ArtistRecievedFromDBEvent?.Invoke(dbContext.Artists.Find(DBIDs[i]));
                        }
                        break;
                }
            }
        }
    }
}
