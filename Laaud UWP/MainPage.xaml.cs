using Laaud_UWP.Models;
using Laaud_UWP.Util;
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

        public MainPage()
        {
            this.InitializeComponent();

            this.SearchResults.ItemsSource = this.searchedSongs;
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
                        List<Song> songs = dbContext.Songs.Where(song => song.Title.ContainsIgnoreCase(searchText)).ToList();
                        
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
            LibraryLoader library = new LibraryLoader();
            library.AddPath(folder);
            library.ReloadAllPaths();
        }
    }
}
