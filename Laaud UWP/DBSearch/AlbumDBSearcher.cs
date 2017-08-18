using Laaud_UWP.Models;
using Laaud_UWP.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.DBSearch
{
    class AlbumDBSearcher : DBSearcher<Album, string>
    {
        protected override List<Album> GetByPrimaryKeys(List<int> primaryKeyCollection)
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                List<Album> albums = dbContext.Albums
                    .Include(album => album.Songs)
                    .Where(album => primaryKeyCollection.Contains(album.AlbumId))
                    .ToList();

                albums.ForEach(album => album.Songs = album.Songs.OrderBy(song => song.Track).ToList());

                return albums;
            }
        }

        protected override List<int> GetItemIdsForSearchTerms(string searchTerm)
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext.Albums
                    .Where(
                        album => album.Name.ContainsIgnoreCase(searchTerm)
                        || album.Artist.Name.ContainsIgnoreCase(searchTerm))
                    .OrderBy(album => album.Name)
                    .Select(album => album.AlbumId)
                    .ToList();
            }
        }

        protected override int GetPrimaryKey(Album loadedItem)
        {
            return loadedItem.AlbumId;
        }
    }
}
