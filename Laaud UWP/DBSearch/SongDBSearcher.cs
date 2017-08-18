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
    class SongDBSearcher : DBSearcher<Song, string>
    {
        protected override List<Song> GetByPrimaryKeys(List<int> primaryKeyCollection)
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext.Songs
                    .Include(song => song.Album)
                    .Include(song => song.Album.Artist)
                    .Where(song => primaryKeyCollection.Contains(song.SongId))
                    .ToList();
            }
        }

        protected override List<int> GetItemIdsForSearchTerms(string searchTerm)
        {
            using (MusicLibraryContext dbContext = new MusicLibraryContext())
            {
                return dbContext.Songs
                    .Include(song => song.Album)
                    .ThenInclude(album => album.Artist)
                    .Where(
                        song => song.Title.ContainsIgnoreCase(searchTerm)
                        || song.Album.Name.ContainsIgnoreCase(searchTerm)
                        || song.Album.Artist.Name.ContainsIgnoreCase(searchTerm))
                    .OrderBy(song => song.Title)
                    .Select(song => song.SongId)
                    .ToList();
            }
        }

        protected override int GetPrimaryKey(Song loadedItem)
        {
            return loadedItem.SongId;
        }
    }
}
