using Laaud_UWP.Models;
using Laaud_UWP.Util;
using Laaud_UWP.LibraryLoader;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Laaud_UWP
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Song> searchedSongs = new ObservableCollection<Song>();
        private CancellationTokenSource loadingSongsCancellationTokenSource = null;
        private readonly TracklistPlayer.TracklistPlayer tracklistPlayer;

        public MainPage()
        {
            this.InitializeComponent();

            this.tracklistPlayer = new TracklistPlayer.TracklistPlayer(this.mediaElement);
            this.tracklistPlayer.SongChanged += this.TracklistPlayer_SongChanged;

            this.SearchResults.ItemsSource = this.searchedSongs;
            this.TrackList.ItemsSource = this.tracklistPlayer.TrackList;
            this.TracklistPlayerControl.SetTracklistPlayer(this.tracklistPlayer);
        }

        private void TracklistPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            this.TrackList.SelectedIndex = e.NewSongIndex;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.searchedSongs.Clear();

            string searchText = this.SearchTextBox.Text;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (this.loadingSongsCancellationTokenSource != null)
                {
                    this.loadingSongsCancellationTokenSource.Cancel();
                }

                Task.Factory.StartNew(async () =>
                {
                    this.loadingSongsCancellationTokenSource = new CancellationTokenSource();
                    await this.Dispatcher.RunAsync(
                        CoreDispatcherPriority.High,
                        new DispatchedHandler(() => this.searchedSongs.Clear()));

                    using (MusicLibraryContext dbContext = new MusicLibraryContext())
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        List<Song> songs = dbContext.Songs
                            .Include(song => song.Album)
                            .ThenInclude(album => album.Artist)
                            .Where(
                                song => song.Title.ContainsIgnoreCase(searchText)
                                || song.Album.Name.ContainsIgnoreCase(searchText)
                                || song.Album.Artist.Name.ContainsIgnoreCase(searchText))
                            .OrderBy(song => song.Title)
                            .Take(100)
                            .ToList();

                        Debug.WriteLine(stopwatch.ElapsedMilliseconds);

                        foreach (Song song in songs)
                        {
                            if (this.loadingSongsCancellationTokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            await this.Dispatcher.RunAsync(
                                CoreDispatcherPriority.Low,
                                new DispatchedHandler(() => this.searchedSongs.Add(song)));
                        }

                        stopwatch.Stop();
                        Debug.WriteLine(stopwatch.ElapsedMilliseconds);
                    }
                });
            }
        }

        private async void AddMusicToLibrary_ClickAsync(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.MusicLibrary };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                LibraryLoader.LibraryLoader libraryLoader = new LibraryLoader.LibraryLoader();
                libraryLoader.ProgressUpdated += this.LibraryLoader_ProgressUpdated;
                this.AddingSongsToLibraryContainer.Visibility = Visibility.Visible;

                libraryLoader.AddPath(folder);
                await libraryLoader.ReloadAllPathsAsync();

                this.AddingSongsToLibraryContainer.Visibility = Visibility.Collapsed;
            }
        }


        private async void LibraryLoader_ProgressUpdated(object sender, LibraryLoadProgressUpdateArgs e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, new DispatchedHandler(() =>
            {
                if (e.LastSongAdded == null)
                {
                    this.AddingSongsToLibraryCurrentSong.Text = "Counting files";
                    this.AddingSongsToLibraryProgressBar.IsIndeterminate = true;
                }
                else
                {
                    if (e.LastSongAdded.Album != null && e.LastSongAdded.Album.Artist != null)
                    {
                        this.AddingSongsToLibraryCurrentSong.Text = string.Format("Adding {0} - {1}", e.LastSongAdded.Album.Artist.Name, e.LastSongAdded.Album.Name);
                        if (this.AddingSongsToLibraryProgressBar.IsIndeterminate)
                        {
                            this.AddingSongsToLibraryProgressBar.IsIndeterminate = false;
                        }

                        this.AddingSongsToLibraryProgressBar.Value = e.Progress;
                    }
                }
            }));
        }

        private void TrackList_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private void TrackList_Drop(object sender, DragEventArgs e)
        {
            Song song = (Song)e.DataView.Properties["song"];

            this.tracklistPlayer.AddSong(song);
        }

        private void TrackList_ItemClick(object sender, ItemClickEventArgs e)
        {
            // play a song when an item is clicked
            if (e.ClickedItem != null)
            {
                this.tracklistPlayer.Play(this.tracklistPlayer.TrackList.IndexOf((Song)e.ClickedItem));
            }
        }
    }
}
