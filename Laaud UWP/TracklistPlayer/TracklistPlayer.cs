using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Laaud_UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Laaud_UWP.TracklistPlayer
{
    public class TracklistPlayer : INotifyPropertyChanged
    {
        private static readonly List<RepeatMode> repeatModes = new List<RepeatMode>() { RepeatMode.NoRepeat, RepeatMode.RepeatPlaylist, RepeatMode.RepeatSong };
        private readonly Random random = new Random();
        private readonly MediaElement player;
        private readonly NotificationChainManager notificationChainManager = new NotificationChainManager();
        private readonly SystemMediaTransportControls systemMediaControls;

        private bool playing;
        private int currentSongIndex;
        private RepeatMode repeatMode;
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
                this.SongChanged(this, new SongChangedEventArgs(this.CurrentSong, this.CurrentSongIndex));
                this.RaisePropertyChanged(nameof(CurrentSongIndex));
            }
        }

        public ICommand PlayPauseCommand { get; }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public ICommand RepeatCommand { get; }

        public ObservableCollection<Song> TrackList { get; }

        public RepeatMode RepeatMode
        {
            get
            {
                return this.repeatMode;
            }

            set
            {
                this.repeatMode = value;
                this.RaisePropertyChanged(nameof(RepeatMode));
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

        #region Calculated properties

        public Song CurrentSong
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.CurrentSongIndex))
                    .Finish();

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

        public string PlayButtonText
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.Playing))
                    .Finish();

                if (this.Playing)
                {
                    return "Pause";
                }
                else
                {
                    return "Play";
                }
            }
        }

        public string RepeatButtonText
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.RepeatMode))
                    .Finish();

                switch (this.RepeatMode)
                {
                    case RepeatMode.NoRepeat:
                        return "No repeat";
                    case RepeatMode.RepeatPlaylist:
                        return "Repeat playlist";
                    case RepeatMode.RepeatSong:
                        return "Repeat song";
                    default:
                        throw new Exception("Unknown RepeatMode");
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public event EventHandler<SongChangedEventArgs> SongChanged = delegate { };

        public TracklistPlayer(MediaElement mediaElement)
        {
            this.notificationChainManager.Observe(this);
            this.notificationChainManager.AddDefaultCall((sender, notifyingProperty, dependentProperty) => RaisePropertyChanged(dependentProperty));

            this.player = mediaElement;
            this.player.CurrentStateChanged += this.Player_CurrentStateChanged;
            this.player.MediaEnded += this.Player_MediaEnded;

            this.systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            this.systemMediaControls.IsPlayEnabled = true;
            this.systemMediaControls.IsPauseEnabled = true;
            this.systemMediaControls.IsNextEnabled = true;
            this.systemMediaControls.IsPreviousEnabled = true;
            this.systemMediaControls.ButtonPressed += this.SystemMediaControls_ButtonPressedAsync;

            this.TrackList = new ObservableCollection<Song>();
            this.PlayPauseCommand = new DelegateCommand(this.PlayPause);
            this.NextCommand = new DelegateCommand(() => this.NextSong(false));
            this.PreviousCommand = new DelegateCommand(this.PreviousSong);
            this.RepeatCommand = new DelegateCommand(this.ToggleRepeat);
        }

        private async void SystemMediaControls_ButtonPressedAsync(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await this.player.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        this.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        this.Pause();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        this.NextSong(false);
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        this.PreviousSong();
                        break;
                }
            });
        }

        private void Player_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.NextSong(true);
        }

        private void Player_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (this.player.CurrentState)
            {
                case MediaElementState.Closed:
                case MediaElementState.Paused:
                case MediaElementState.Stopped:
                    this.Playing = false;
                    break;
                case MediaElementState.Buffering:
                case MediaElementState.Opening:
                case MediaElementState.Playing:
                    this.Playing = true;
                    break;
            }

            switch (this.player.CurrentState)
            {
                case MediaElementState.Playing:
                    this.systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaElementState.Opening:
                case MediaElementState.Buffering:
                    this.systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaElementState.Paused:
                    this.systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaElementState.Closed:
                    this.systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
            }
        }

        public void AddSong(Song song)
        {
            this.TrackList.Add(song);

            if (this.TrackList.Count == 1)
            {
                // after adding the first song, set it as the current one
                this.SetSong(0);
            }
        }

        public void RemoveSong(int songIndex)
        {
            this.TrackList.RemoveAt(songIndex);

            if (songIndex == this.CurrentSongIndex)
            {
                this.NextSong(false);
            }
        }

        public void PreviousSong()
        {
            if (this.TrackList.Count > 0)
            {
                if (this.player.Position.Seconds > 3 || this.CurrentSongIndex == 0)
                {
                    this.player.Position = TimeSpan.Zero;
                }
                else if (this.CurrentSongIndex != 0)
                {
                    this.Play(this.CurrentSongIndex - 1);
                }
            }
        }

        public void NextSong(bool respectRepeat)
        {
            if (this.TrackList.Count > 0)
            {
                if (respectRepeat && this.RepeatMode == RepeatMode.RepeatSong)
                {
                    this.Play();
                }
                else if (this.CurrentSongIndex == this.TrackList.Count - 1)
                {
                    if (respectRepeat)
                    {
                        if (this.RepeatMode == RepeatMode.RepeatPlaylist)
                        {
                            this.Play(0);
                        }
                        else
                        {
                            this.SetSong(0);
                        }
                    }
                    else
                    {
                        this.Play(0);
                    }

                }
                else
                {
                    this.Play(this.CurrentSongIndex + 1);
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

        public void Pause()
        {
            this.player.Pause();
        }

        public void Stop()
        {
            this.player.Stop();
        }

        public void StopAndClear()
        {
            this.Stop();
            this.CurrentSongIndex = 0;
        }

        public void Play()
        {
            this.Play(this.CurrentSongIndex);
        }

        public void Play(int index)
        {
            if (this.TrackList.Count > 0)
            {
                if (this.player.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Closed || this.CurrentSongIndex != index)
                {
                    this.CurrentSongIndex = index;
                    this.LoadCurrentSongToPlayer();
                }

                this.player.Play();
            }
        }

        private void SetSong(int index)
        {
            this.CurrentSongIndex = index;
        }

        private void ToggleRepeat()
        {
            int currentModeIndex = repeatModes.IndexOf(this.RepeatMode);
            int nextModeIndex = currentModeIndex + 1;
            if (nextModeIndex >= repeatModes.Count)
            {
                nextModeIndex = 0;
            }

            this.RepeatMode = repeatModes[nextModeIndex];
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
