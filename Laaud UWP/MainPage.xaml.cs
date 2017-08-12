using Laaud_UWP.Models;
using Laaud_UWP.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private readonly TracklistPlayer tracklistPlayer = new TracklistPlayer();

        public MainPage()
        {
            this.InitializeComponent();

            this.SearchResults.ItemsSource = this.searchedSongs;
            this.TrackList.ItemsSource = this.tracklistPlayer.TrackList;
            this.TracklistPlayerControl.SetTracklistPlayer(this.tracklistPlayer);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.searchedSongs.Clear();

            string searchText = this.SearchTextBox.Text;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                Task.Factory.StartNew(async () =>
                {
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
                            .ToList();
                        
                        Debug.WriteLine(stopwatch.ElapsedMilliseconds);

                        foreach (Song song in songs)
                        {
                            await this.Dispatcher.RunAsync(
                                CoreDispatcherPriority.Normal, 
                                new DispatchedHandler(() => this.searchedSongs.Add(song)));
                        }

                        stopwatch.Stop();
                        Debug.WriteLine(stopwatch.ElapsedMilliseconds);
                    }
                });
            }
        }

        private void AddMusicToLibrary_Click(object sender, RoutedEventArgs e)
        {
            IndexLibraryAsync();
        }

        public async void IndexLibraryAsync()
        {
            FolderPicker folderPicker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.MusicLibrary };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                LibraryLoader library = new LibraryLoader();
                library.AddPath(folder);
                library.ReloadAllPaths();
            }
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

        private async void TrackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Song clickedSong = (Song)e.AddedItems.First();
        }
    }
}
