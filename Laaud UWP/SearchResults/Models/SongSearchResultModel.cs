using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Laaud_UWP.SearchResults.Models
{
    class SongSearchResultModel : ISearchResultModel
    {
        private readonly Song data;

        public SongSearchResultModel(Song data)
        {
            this.data = data;
        }

        public int Id
        {
            get
            {
                return this.data.SongId;
            }
        }

        public string Title
        {
            get
            {
                return this.data.Title;
            }
        }

        public bool Favorite
        {
            get
            {
                return this.data.Favorite;
            }

            set
            {
                this.data.Favorite = value;
                Task.Factory.StartNew(() => this.SaveChanges());
            }
        }

        public bool HasChildren
        {
            get
            {
                return false;
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
            throw new InvalidOperationException();
        }

        public async Task<ImageSource> LoadImageAsync()
        {
            return await SongImageUtil.LoadImageAsync(this.data.SongId);
        }

        private void SaveChanges()
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                dbContext.Attach(this.data);
                dbContext.SaveChanges();
            }
        }
    }
}
