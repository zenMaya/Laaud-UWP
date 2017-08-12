using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Models
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        [Required]
        public string Name { get; set; }

        public List<Song> Songs { get; set; }
    }
}
