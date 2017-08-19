using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.SearchResults.Models
{
    class AlbumSearchResultModel : ISearchResultModel
    {
        private readonly Album data;
        private bool favorite;

        public AlbumSearchResultModel(Album data)
        {
            this.data = data;

            this.favorite = this.LoadFavorite();
        }

        public int Id
        {
            get
            {
                return this.data.AlbumId;
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
                Task.Factory.StartNew(() => FavoriteAllSongs(this.favorite));
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
            this.LoadSongs();

            return this.data.Songs.Select(song => new SongSearchResultModel(song));
        }

        private bool LoadFavorite()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext
                    .Attach(this.data)
                    .Collection(album => album.Songs)
                    .Query()
                    .All(song => song.Favorite);
            }
        }

        private void FavoriteAllSongs(bool favorite)
        {
            this.LoadSongs();

            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                foreach (Song song in this.data.Songs)
                {
                    song.Favorite = favorite;

                    dbContext
                        .Attach(song)
                        .Property(_song => _song.Favorite)
                        .IsModified = true;
                }

                dbContext.SaveChanges();
            }
        }

        private void LoadSongs()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                dbContext
                    .Attach(this.data)
                    .Collection(album => album.Songs)
                    .Load();
            }
        }
    }
}
