using Com.PhilChuang.Utils.MvvmNotificationChainer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Laaud_UWP.SearchResults
{
    public sealed partial class SearchResult : UserControl, INotifyPropertyChanged
    {
        private readonly NotificationChainManager notificationChainManager = new NotificationChainManager();

        private string title;
        private bool favorite;
        private bool expanderToggleState;
        private SearchResultSelectedChangedEventArgs selectedItem;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SearchResultSelectedChangedEventArgs> SelectedChanged;
        public event EventHandler<SearchResultEventArgs> DoubleClick;
        public event EventHandler<SearchResultFavoriteChangedEventArgs> FavoriteChanged;

        public int Id { get; }

        public int Level { get; }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.RaisePropertyChanged(nameof(Title));
            }
        }

        public bool Favorite
        {
            get
            {
                return this.favorite;
            }

            set
            {
                this.favorite = value;
                this.RaisePropertyChanged(nameof(Favorite));
                this.FavoriteChanged?.Invoke(this, new SearchResultFavoriteChangedEventArgs(this, this.Favorite));
            }
        }

        public bool ShowTitleBar
        {
            get
            {
                return this.Level > 0;
            }
        }

        public bool ExpanderToggleState
        {
            get
            {
                return this.expanderToggleState;
            }

            set
            {
                this.expanderToggleState = value;
                this.RaisePropertyChanged(nameof(ExpanderToggleState));
            }
        }

        public bool ShowExpanderButton
        {
            get
            {
                return this.ChildrenListView.Items.Count > 0;
            }
        }

        public bool ShowExpanderList
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.ExpanderToggleState))
                    .Finish();

                return this.Level == 0 || (this.ChildrenListView.Items.Count > 0 && this.ExpanderToggleState);
            }
        }

        public BitmapImage FavoriteImageSource
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.Favorite))
                    .Finish();

                if (this.Favorite)
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/Favorites.png", UriKind.Absolute));
                }
                else
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/AddMusicToLibrary.png", UriKind.Absolute));
                }
            }
        }

        public BitmapImage ExpanderImageSource
        {
            get
            {
                this.notificationChainManager
                    .CreateOrGet()
                    .Configure(c => c.On(() => this.ExpanderToggleState))
                    .Finish();

                if (this.ExpanderToggleState)
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/Settings.png", UriKind.Absolute));
                }
                else
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/Laaud Logo.png", UriKind.Absolute));
                }
            }
        }

        private SearchResult(int id, int level, string title, bool favorite)
        {
            this.notificationChainManager.Observe(this);
            this.notificationChainManager.AddDefaultCall((sender, notifyingProperty, dependentProperty) => RaisePropertyChanged(dependentProperty));

            this.Id = id;
            this.Level = level;

            this.InitializeComponent();

            this.DataContext = this;

            this.Title = title;
            this.Favorite = favorite;
        }

        public SearchResult()
            : this(0, 0, string.Empty, false)
        {

        }

        public SearchResult AddChild(int id, string title, bool favorite)
        {
            SearchResult searchResult = new SearchResult(id, this.Level + 1, title, favorite);
            searchResult.SelectedChanged += this.ProcessSelectionChanged;
            searchResult.DoubleClick += this.DoubleClick;
            searchResult.FavoriteChanged += this.FavoriteChanged;

            this.ChildrenListView.Items.Add(searchResult);

            this.RaisePropertyChanged(nameof(ShowExpanderButton));
            this.RaisePropertyChanged(nameof(ShowExpanderList));

            return searchResult;
        }

        private void ProcessSelectionChanged(object sender, SearchResultSelectedChangedEventArgs e)
        {
            if (this.Level == 0)
            {
                if (this.selectedItem != null && this.selectedItem.ParentList != e.ParentList)
                {
                    this.selectedItem.ParentList.SelectionMode = ListViewSelectionMode.None;
                }

                if (e.IsSelected)
                {
                    this.selectedItem = e;
                }
                else
                {
                    this.selectedItem = null;
                }

                this.SelectedChanged?.Invoke(sender, e);
            }
            else
            {
                this.SelectedChanged?.Invoke(sender, e);
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            this.Favorite = !this.Favorite;
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            this.ExpanderToggleState = !this.ExpanderToggleState;
        }

        private void ChildrenListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != null)
            {
                ListView list = sender as ListView;
                ListViewItem listItem = list.ContainerFromItem(e.ClickedItem) as ListViewItem;

                if (listItem.IsSelected)
                {
                    listItem.IsSelected = false;
                    list.SelectionMode = ListViewSelectionMode.None;
                }
                else
                {
                    list.SelectionMode = ListViewSelectionMode.Single;
                    listItem.IsSelected = true;
                }

                this.ProcessSelectionChanged(this, new SearchResultSelectedChangedEventArgs(this, this.ChildrenListView, listItem.IsSelected));
            }
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.DoubleClick?.Invoke(this, new SearchResultEventArgs(this));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
