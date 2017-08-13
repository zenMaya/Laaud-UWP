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
        private List<int> dbIds = new List<int>();
        private SearchResultsGroupType groupType;

        public event Action<Song> SongReceivedFromDBEvent;
        public event Action<Album> AlbumReceivedFromDBEvent;
        public event Action<Artist> ArtistReceivedFromDBEvent;

        public async Task SearchAsync(SearchResultsGroupType groupType, string[] phrases = null)
        {
            if (this.loadingSongsCancellationTokenSource != null)
            {
                this.loadingSongsCancellationTokenSource.Cancel();
            }

            await Task.Factory.StartNew(() =>
            {
                this.groupType = groupType;
                this.dbIds.Clear();

                this.loadingSongsCancellationTokenSource = new CancellationTokenSource();

                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string phrase = phrases != null ? phrases[0] : string.Empty;

                    switch (this.groupType)
                    {
                        case SearchResultsGroupType.Song:

                            this.dbIds = dbContext.Songs
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



                        case SearchResultsGroupType.Album:

                            this.dbIds = dbContext.Songs
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

                        case SearchResultsGroupType.Artist:
                            this.dbIds = dbContext.Songs
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
                }
            });
        }

        public void GetSearchResults(int firstElement, int range)
        {
            Task.Factory.StartNew(() =>
            {
                //int maxVal = range + firstElement >= this.dbIds.Count ? this.dbIds.Count - 1 : range + firstElement;
                List<int> idsToFetch = this.dbIds.Skip(firstElement + 1).Take(range).ToList();
                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    switch (this.groupType)
                    {
                        case SearchResultsGroupType.Song:
                            foreach (Song song in dbContext.Songs
                                    .Where(song => idsToFetch.Contains(song.SongId)))
                            {
                                this.SongReceivedFromDBEvent?.Invoke(song);
                            }
                            break;

                        case SearchResultsGroupType.Album:
                            foreach (Album album in dbContext.Albums
                                    .Include(album => album.Songs)
                                    .Where(album => idsToFetch.Contains(album.AlbumId)))
                            {
                                this.AlbumReceivedFromDBEvent?.Invoke(album);
                            }
                            break;

                        case SearchResultsGroupType.Artist:
                            foreach (Artist artist in dbContext.Artists
                                    .Include(artist => artist.Albums)
                                    .Include("Albums.Songs")
                                    .Where(artist => idsToFetch.Contains(artist.ArtistId)))
                            {
                                this.ArtistReceivedFromDBEvent?.Invoke(artist);
                            }
                            break;
                    }
                }
            });
        }
    }
}
