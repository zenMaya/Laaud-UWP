using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Models
{
    public class Album
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }

        public Artist Artist { get; set; }
        public int ArtistId { get; set; }
        public List<Song> Songs
        {
            get;
            set;
        }

        public static Album CreateOrFind(MusicLibraryContext dbContext, string name, int artistId)
        {
            Album album = dbContext.Albums.FirstOrDefault(_album => _album.Name == name && _album.ArtistId == artistId);
            if (album == null)
            {
                // if not found, create a new one
                album = new Album()
                {
                    Name = name,
                    ArtistId = artistId
                };

                dbContext.Albums.Add(album);
            }
            else
            {
                dbContext.Albums.Attach(album);
            }

            return album;
        }
    }
}
