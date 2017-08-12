using Laaud_UWP.Models;
using System;

namespace Laaud_UWP
{
    public class SongChangedEventArgs : EventArgs
    {
        public Song NewSong { get; }
        public int NewSongIndex { get; }

        public SongChangedEventArgs(Song newSong, int newSongIndex)
        {
            this.NewSong = newSong;
            this.NewSongIndex = newSongIndex;
        }
    }
}