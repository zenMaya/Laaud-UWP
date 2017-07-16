using System.Collections.Generic;

namespace Laaud_UWP.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
       
        public List<Album> Albums { get; set; }
    }
}