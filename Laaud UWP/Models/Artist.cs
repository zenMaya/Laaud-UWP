using System.Collections.Generic;
using System.Linq;

namespace Laaud_UWP.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }

        public List<Album> Albums { get; set; }

        public static Artist CreateOrFind(MusicLibraryContext dbContext, string name)
        {
            Artist artist = dbContext.Artists.FirstOrDefault(_artist => _artist.Name == name);
            if (artist == null)
            {
                // if not found, create a new one
                artist = new Artist()
                {
                    Name = name
                };

                dbContext.Artists.Add(artist);
            }
            else
            {
                dbContext.Artists.Attach(artist);
            }

            return artist;
        }
    }
}