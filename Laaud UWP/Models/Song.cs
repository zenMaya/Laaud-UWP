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

        [NotMapped]
        public StorageFile File { get; set; }

        public Album Album { get; set; }
        public int AlbumId { get; set; }

        public List<PlaylistItem> PlaylistItems { get; set; }
    }
}
