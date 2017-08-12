using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Models
{
    public class PlaylistItem
    {
        public int PlaylistItemId { get; set; }
        public int OrderInList { get; set; }

        public int SongId { get; set; }
        public Song Song { get; set; }

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; }
    }
}
