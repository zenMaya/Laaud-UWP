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
    }
}
