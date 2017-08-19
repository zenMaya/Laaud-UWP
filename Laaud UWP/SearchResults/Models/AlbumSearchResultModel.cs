using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Laaud_UWP.SearchResults.Models
{
    class AlbumSearchResultModel : ISearchResultModel
    {
        private readonly Album data;
        private bool favorite;
        private bool anySongs;

        public AlbumSearchResultModel(Album data)
        {
            this.data = data;

            this.anySongs = this.HasAnySongs();
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
                return this.anySongs;
            }
        }

        public bool HasImage
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

        public async Task<ImageSource> LoadImageAsync()
        {
            if (this.anySongs)
            {
                Song firstSong;
                using (MusicLibraryContext dbContext = new MusicLibraryContext())
                {
                    firstSong = dbContext
                        .Songs
                        .Where(song => song.AlbumId == this.data.AlbumId)
                        .First();
                }

                return await SongImageUtil.LoadImageAsync(firstSong.SongId);
            }

            return ImageUtil.GetAssetsBitmapImageByFileName("Favor.png");
        }

        private bool LoadFavorite()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return this.anySongs
                    && dbContext
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

        private bool HasAnySongs()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext
                            .Entry(this.data)
                            .Collection(album => album.Songs)
                            .Query()
                            .Any();
            }
        }
    }
}
