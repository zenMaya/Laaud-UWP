using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace Laaud_UWP
{
    public class TracklistPlayer : INotifyPropertyChanged
    {
        private readonly MediaElement player = new MediaElement();
        private readonly Random random = new Random();

        private bool playing;
        private int currentSongIndex;
        private bool repeat;
        private bool shuffle;

        public bool Playing
        {
            get
            {
                return this.playing;
            }

            private set
            {
                this.playing = value;
                this.RaisePropertyChanged(nameof(Playing));
            }
        }

        public int CurrentSongIndex
        {
            get
            {
                return this.currentSongIndex;
            }

            private set
            {
                this.currentSongIndex = value;
                this.SongChanged(this, new SongChangedEventArgs(this.CurrentSong));
                this.RaisePropertyChanged(nameof(CurrentSongIndex));
                this.RaisePropertyChanged(nameof(CurrentSong));
            }
        }

        public Song CurrentSong
        {
            get
            {
                if (this.CurrentSongIndex < this.TrackList.Count)
                {
                    return this.TrackList[this.CurrentSongIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        public ICommand PlayPauseCommand { get; }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public ObservableCollection<Song> TrackList { get; }

        public bool Repeat
        {
            get
            {
                return this.repeat;
            }

            set
            {
                this.repeat = value;
            }
        }

        public bool Shuffle
        {
            get
            {
                return this.shuffle;
            }

            set
            {
                this.shuffle = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public event EventHandler<SongChangedEventArgs> SongChanged = delegate { };

        public TracklistPlayer()
        {
            this.TrackList = new ObservableCollection<Song>();
            this.PlayPauseCommand = new DelegateCommand(this.PlayPause);
            this.NextCommand = new DelegateCommand(this.NextSong);
            this.PreviousCommand = new DelegateCommand(this.PreviousSong);
        }

        public void AddSong(Song song)
        {
            this.TrackList.Add(song);
        }

        public void RemoveSong(int songIndex)
        {
            this.TrackList.RemoveAt(songIndex);

            if (songIndex == this.CurrentSongIndex)
            {
                this.NextSong();
            }
        }

        public void PreviousSong()
        {
            if (this.TrackList.Count > 0)
            {
                if (this.player.Position.Seconds > 3 || this.CurrentSongIndex == 0)
                {
                    // TODO reset position to the beginning instead of playing previous song
                    // remove the code below when implementing
                    if (this.CurrentSongIndex != 0)
                    {
                        this.SetAndPlay(this.CurrentSongIndex - 1);
                    }
                }
                else if (this.CurrentSongIndex != 0)
                {
                    this.SetAndPlay(this.CurrentSongIndex - 1);
                }
            }
        }

        public void NextSong()
        {
            if (this.TrackList.Count > 0)
            {
                if (this.Shuffle)
                {
                    this.SetAndPlay(this.random.Next(this.TrackList.Count - 1));
                }
                else if (this.CurrentSongIndex == this.TrackList.Count - 1)
                {
                    this.SetAndPlay(0);
                }
                else
                {
                    this.SetAndPlay(this.CurrentSongIndex + 1);
                }
            }
        }

        public void PlayPause()
        {
            if (this.Playing)
            {
                this.Pause();
            }
            else
            {
                this.Play();
            }
        }

        public void Play()
        {
            this.player.Play();
            this.Playing = true;
        }

        public void Pause()
        {
            this.player.Pause();
            this.Playing = false;
        }

        public void Stop()
        {
            this.player.Stop();
            this.Playing = false;
        }

        public void StopAndClear()
        {
            this.Stop();
            this.CurrentSongIndex = 0;
        }

        public void SetAndPlay(int index)
        {
            this.CurrentSongIndex = index;
            this.LoadCurrentSongToPlayer();
            this.Play();
        }

        private async void LoadCurrentSongToPlayer()
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(this.CurrentSong.Path);
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            this.player.SetSource(stream, file.ContentType);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
