using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.LibraryLoader
{
    class LibraryLoadProgressUpdateArgs : EventArgs
    {
        public float Progress { get; }
        public Song LastSongAdded { get; }

        public LibraryLoadProgressUpdateArgs(float progress, Song lastSongAdded)
        {
            this.Progress = progress;
            this.LastSongAdded = lastSongAdded;
        }
    }
}
