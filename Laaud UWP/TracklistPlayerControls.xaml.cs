using Laaud_UWP.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class TracklistPlayerControls : UserControl
    {
        public TracklistPlayerControls()
        {
            this.InitializeComponent();
        }

        public void SetTracklistPlayer(TracklistPlayer.TracklistPlayer tracklistPlayer)
        {
            this.DataContext = tracklistPlayer;
            tracklistPlayer.SongChanged += (sender, args) => this.LoadImageForSong(args.NewSong);
        }

        private async void LoadImageForSong(Song song)
        {
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
