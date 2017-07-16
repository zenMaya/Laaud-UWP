using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Album Album { get; set; }
        public int AlbumId { get; set; }

        public void ReadFromTag(TagLibUWP.Tag tag, MusicLibraryContext dbContext)
        {
            // search for an existing artist by name
            Artist artist = dbContext.Artists.FirstOrDefault(_artist => _artist.Name == tag.Artist);
            if (artist == null)
            {
                // if not found, create a new one
                artist = new Artist();
                artist.Name = tag.Artist;
                dbContext.Artists.Add(artist);
                dbContext.SaveChanges();
            }

            // search for an existing album by name
            Album existingAlbum = dbContext.Albums.FirstOrDefault(album => album.Name == tag.Album);
            if (existingAlbum == null)
            {
                // if not found, create a new one
                Album newAlbum = new Album();
                newAlbum.Name = tag.Album;
                newAlbum.Artist = artist;
                newAlbum.ArtistId = artist.ArtistId;
                dbContext.Albums.Add(newAlbum);
                dbContext.SaveChanges();
                this.Album = newAlbum;
                this.AlbumId = newAlbum.AlbumId;
            }
            else
            {
                this.Album = existingAlbum;
                this.AlbumId = existingAlbum.AlbumId;
            }

            // load other simpler properties
            this.Year = (int)tag.Year;
            this.Track = (int)tag.Track;
            this.Title = tag.Title;
            this.Genre = tag.Genre;
            this.Comment = tag.Comment;
        }
    }
}
