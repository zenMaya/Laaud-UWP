using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Laaud_UWP
{
    public sealed partial class SearchResultSong : UserControl
    {
        public SearchResultSong()
        {
            this.InitializeComponent();
        }

        private async void UserControl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            Song song = (Song)this.DataContext;

            List<IStorageItem> files = new List<IStorageItem>();
            StorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
            files.Add(file);

            DataPackage package = args.Data;

            package.RequestedOperation = DataPackageOperation.Copy;
            package.Properties.Description = song.Title;
            package.Properties.Add("song", song);
            package.SetStorageItems(files);
        }

        private async void SongImage_Loaded(object sender, RoutedEventArgs e)
        {
            Song song = (Song)this.DataContext;
            if (song != null)
            {
                BitmapImage image = new BitmapImage();
                StorageFile imageFile = await SongImageUtil.LoadImageAsync(song.SongId);
                if (imageFile != null)
                {
                    await image.SetSourceAsync(await imageFile.OpenReadAsync());
                    this.SongImage.Source = image;
                }
            }
        }
    }
}
