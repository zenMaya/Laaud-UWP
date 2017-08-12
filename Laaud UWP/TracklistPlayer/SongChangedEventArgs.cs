using Laaud_UWP.Models;
using System;

namespace Laaud_UWP
{
    public class SongChangedEventArgs : EventArgs
    {
        public Song NewSong { get; }

        public SongChangedEventArgs(Song newSong)
        {
            this.NewSong = newSong;
        }
    }
}