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
using Laaud_UWP.SearchResults;

namespace Laaud_UWP
{
    public sealed partial class MainPage : Page
    {
        private static readonly List<SearchResultsGroupType> groupTypesOrder = new List<SearchResultsGroupType>() { SearchResultsGroupType.Song, SearchResultsGroupType.Album, SearchResultsGroupType.Artist };

        private readonly TracklistPlayer.TracklistPlayer tracklistPlayer;
        private readonly SongDBSearcher songDBSearcher;

        private SearchResultsGroupType currentGroupType;
        private SearchResults.SearchResult topSearchResult;
        private Dictionary<int, Song> searchedSongsCache = new Dictionary<int, Song>();
        private Dictionary<int, Album> searchedAlbumsCache = new Dictionary<int, Album>();
        private Dictionary<int, Artist> searchedArtistsCache = new Dictionary<int, Artist>();

        public SearchResultsGroupType CurrentGroupType
        {
            get
            {
                return this.currentGroupType;
            }

            set
            {
                this.currentGroupType = value;
                switch (this.CurrentGroupType)
                {
                    case SearchResultsGroupType.Song:
                        this.ChangeViewType.Content = "S";
                        break;
                    case SearchResultsGroupType.Album:
                        this.ChangeViewType.Content = "AS";
                        break;
                    case SearchResultsGroupType.Artist:
                        this.ChangeViewType.Content = "IAS";
                        break;
                }
            }
        }


        public MainPage()
        {
            this.InitializeComponent();

            this.CurrentGroupType = SearchResultsGroupType.Song;

            this.tracklistPlayer = new TracklistPlayer.TracklistPlayer(this.mediaElement);
            this.tracklistPlayer.SongChanged += this.TracklistPlayer_SongChanged;

            this.songDBSearcher = new SongDBSearcher();
            this.songDBSearcher.SongReceivedFromDBEvent += (song) => this.AddSongToSearchResults(song, this.topSearchResult);
            this.songDBSearcher.AlbumReceivedFromDBEvent += (album) => this.AddAlbumToSearchResults(album, this.topSearchResult);
            this.songDBSearcher.ArtistReceivedFromDBEvent += (artist) => this.AddArtistToSearchResults(artist, this.topSearchResult);

            this.TrackList.ItemsSource = this.tracklistPlayer.TrackList;
            this.TracklistPlayerControl.SetTracklistPlayer(this.tracklistPlayer);
        }

        private async void AddArtistToSearchResults(Artist obj, SearchResult parentSearchResult)
        {
            await this.Dispatcher.RunIdleAsync((args) =>
            {
                if (!this.searchedArtistsCache.ContainsKey(obj.ArtistId))
                {
                    SearchResult artistSearchResult = parentSearchResult.AddChild(obj.ArtistId, obj.Name, false);
                    this.searchedArtistsCache.Add(obj.ArtistId, obj);

                    foreach (Album album in obj.Albums)
                    {
                        this.AddAlbumToSearchResults(album, artistSearchResult);
                    }
                }
            });
        }

        private async void AddAlbumToSearchResults(Album obj, SearchResult parentSearchResult)
        {
            await this.Dispatcher.RunIdleAsync((args) =>
            {
                if (!this.searchedAlbumsCache.ContainsKey(obj.AlbumId))
                {
                    SearchResult albumSearchResult = parentSearchResult.AddChild(obj.AlbumId, obj.Name, false);
                    this.searchedAlbumsCache.Add(obj.AlbumId, obj);

                    foreach (Song song in obj.Songs)
                    {
                        this.AddSongToSearchResults(song, albumSearchResult);
                    }
                }
            });
        }

        private async void AddSongToSearchResults(Song obj, SearchResult parentSearchResult)
        {
            await this.Dispatcher.RunIdleAsync((args) =>
            {
                if (!this.searchedSongsCache.ContainsKey(obj.SongId))
                {
                    this.searchedSongsCache.Add(obj.SongId, obj);
                    parentSearchResult.AddChild(obj.SongId, obj.Title, false);
                }
            });
        }

        private void TracklistPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            this.TrackList.SelectedIndex = e.NewSongIndex;
        }

        private async void SearchTextBox_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            string searchText = this.SearchTextBox.Text;

            this.topSearchResult = new SearchResults.SearchResult();
            this.topSearchResult.DoubleClick += this.TopSearchResult_DoubleClick;
            this.SearchResults.Content = this.topSearchResult;
            this.searchedSongsCache.Clear();
            this.searchedAlbumsCache.Clear();
            this.searchedArtistsCache.Clear();

            await this.songDBSearcher.SearchAsync(this.CurrentGroupType, searchText.Split(' '));

            this.songDBSearcher.GetSearchResults(0, 50);
        }

        private void TopSearchResult_DoubleClick(object sender, SearchResultEventArgs e)
        {
            if (this.CurrentGroupType == SearchResultsGroupType.Song)
            {
                switch (e.ChangedObject.Level)
                {
                    case 0:
                        break;
                    case 1:
                        this.tracklistPlayer.AddSong(this.searchedSongsCache[e.ChangedObject.Id]);
                        break;
                }
            }
            else if (this.CurrentGroupType == SearchResultsGroupType.Album)
            {
                switch (e.ChangedObject.Level)
                {
                    case 0:
                        break;
                    case 1:
                        foreach(Song song in this.searchedAlbumsCache[e.ChangedObject.Id].Songs)
                        {
                            this.tracklistPlayer.AddSong(song);
                        }
                        break;
                    case 2:
                        this.tracklistPlayer.AddSong(this.searchedSongsCache[e.ChangedObject.Id]);
                        break;
                }
            }
            else if (this.CurrentGroupType == SearchResultsGroupType.Artist)
            {
                switch (e.ChangedObject.Level)
                {
                    case 0:
                        break;
                    case 1:
                        foreach (Album album in this.searchedArtistsCache[e.ChangedObject.Id].Albums)
                        {
                            foreach (Song song in album.Songs)
                            {
                                this.tracklistPlayer.AddSong(song);
                            }
                            break;
                        }
                        break;
                    case 2:
                        foreach (Song song in this.searchedAlbumsCache[e.ChangedObject.Id].Songs)
                        {
                            this.tracklistPlayer.AddSong(song);
                        }
                        break;
                    case 3:
                        this.tracklistPlayer.AddSong(this.searchedSongsCache[e.ChangedObject.Id]);
                        break;
                }
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

        private void ChangeViewType_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentGroupType = groupTypesOrder.GetNextAfter(this.CurrentGroupType);
        }
    }
}
