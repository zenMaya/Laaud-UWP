using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLibUWP;
using Windows.Storage;

namespace Laaud_UWP.Models
{
    public class Song
    {
        public int SongId { get; set; }
        public string Path { get; set; }
        public int Year { get; set; }
        public int Track { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Comment { get; set; }
        public bool Favorite { get; set; }

        [NotMapped]
        public StorageFile File { get; set; }

        public Album Album { get; set; }
        public int AlbumId { get; set; }

        public List<PlaylistItem> PlaylistItems { get; set; }

        public static Song CreateOrFind(MusicLibraryContext dbContext, string path)
        {
            Song song = dbContext.Songs.FirstOrDefault(_song => _song.Path == path);
            if (song == null)
            {
                // if not found, create a new one
                song = new Song()
                {
                    Path = path
                };

                dbContext.Songs.Add(song);
            }
            else
            {
                dbContext.Songs.Attach(song);
            }

            return song;
        }
    }
}
