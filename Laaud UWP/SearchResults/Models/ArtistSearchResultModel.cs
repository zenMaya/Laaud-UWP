using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.SearchResults.Models
{
    class ArtistSearchResultModel : ISearchResultModel
    {
        private readonly Artist data;
        private bool favorite;

        public ArtistSearchResultModel(Artist data)
        {
            this.data = data;

            this.favorite = this.LoadFavorite();
        }

        public int Id
        {
            get
            {
                return this.data.ArtistId;
            }
        }

        public string Title
        {
            get
            {
                return this.data.Name;
            }
        }

        public bool Favorite
        {
            get
            {
                return this.favorite;
            }

            set
            {
                this.favorite = value;

                Task.Factory.StartNew(() => this.FavoriteAllSongs(this.favorite));
            }
        }

        public bool HasChildren
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<ISearchResultModel> CreateChildren()
        {
            this.LoadAlbums();

            return this.data.Albums.Select(album => new AlbumSearchResultModel(album));
        }

        private bool LoadFavorite()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext
                    .Attach(this.data)
                    .Collection(artist => artist.Albums)
                    .Query()
                    .GroupJoin(dbContext.Songs, album => album.AlbumId, song => song.SongId, (album, songs) => songs)
                    .All(songs => songs.All(song => song.Favorite));
            }
        }

        private void FavoriteAllSongs(bool favorite)
        {
            this.LoadAlbums();

            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                foreach (Album album in this.data.Albums)
                {
                    List<Song> songs = dbContext.Songs.Where(song => song.AlbumId == album.AlbumId).ToList();

                    foreach (Song song in songs)
                    {
                        song.Favorite = favorite;

                        dbContext
                            .Attach(song)
                            .Property(_song => _song.Favorite)
                            .IsModified = true;
                    }
                }

                dbContext.SaveChanges();
            }
        }

        private void LoadAlbums()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                dbContext
                    .Attach(this.data)
                    .Collection(artist => artist.Albums)
                    .Load();
            }
        }
    }
}
