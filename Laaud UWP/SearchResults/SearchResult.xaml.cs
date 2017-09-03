using Com.PhilChuang.Utils.MvvmNotificationChainer;
using Laaud_UWP.SearchResults.Models;
using Laaud_UWP.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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

        private bool expanderToggleState;
        private bool loadingChildren;
        private SearchResultSelectedChangedEventArgs selectedItem;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SearchResultSelectedChangedEventArgs> SelectedChanged;
        public event EventHandler<SearchResultEventArgs> DoubleClick;

        private readonly ISearchResultModel model;

        public int Id
        {
            get
            {
                return this.model != null
                    ? this.model.Id
                    : 0;
            }
        }

        public string Title
        {
            get
            {
                return this.model?.Title;
            }
        }

        public int Level { get; }

        public bool Favorite
        {
            get
            {
                return this.model != null
                    ? this.model.Favorite
                    : false;
            }

            private set
            {
                if (this.model != null)
                {
                    this.model.Favorite = value;
                    this.RaisePropertyChanged(nameof(Favorite));
                }
            }
        }

        public bool LoadingChildren
        {
            get
            {
                return this.loadingChildren;
            }

            private set
            {
                this.loadingChildren = value;
            }
        }

        public bool ShowTopBar
        {
            get
            {
                return this.model != null;
            }
        }

        public bool ExpanderToggleState
        {
            get
            {
                return this.expanderToggleState;
            }

            private set
            {
                this.expanderToggleState = value;
                this.RaisePropertyChanged(nameof(ExpanderToggleState));
                if (this.expanderToggleState)
                {
                    LoadAndAddItemsAsync();
                }
            }
        }

        public bool ShowExpanderButton
        {
            get
            {
                return this.model != null
                    ? this.model.HasChildren
                    : false;
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

                return !this.ShowTopBar || (this.model.HasChildren && this.ExpanderToggleState);
            }
        }

        public bool ShowImage
        {
            get
            {
                return this.model != null
                    ? this.model.HasImage
                    : false;
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
                    return ImageUtil.GetAssetsBitmapImageByFileName("Favorites.png");
                }
                else
                {
                    return ImageUtil.GetAssetsBitmapImageByFileName("Settings.png");
                }
            }
        }

        private SearchResult(int level, ISearchResultModel model)
        {
            this.notificationChainManager.Observe(this);
            this.notificationChainManager.AddDefaultCall((sender, notifyingProperty, dependentProperty) => RaisePropertyChanged(dependentProperty));

            this.model = model;

            this.Level = level;

            this.InitializeComponent();

            this.DataContext = this;
        }

        public SearchResult()
            : this(0, null)
        {

        }

        public SearchResult AddChild(ISearchResultModel modelArgument)
        {
            SearchResult searchResult = new SearchResult(this.Level + 1, modelArgument);
            searchResult.SelectedChanged += this.ProcessSelectionChanged;
            searchResult.DoubleClick += this.DoubleClick;

            this.ChildrenListView.Items.Add(searchResult);

            return searchResult;
        }

        private async void LoadAndAddItemsAsync()
        {
            if (this.model != null && !this.LoadingChildren && this.model.HasChildren && this.ChildrenListView.Items.Count == 0)
            {
                this.LoadingChildren = true;
                await Task.Factory.StartNew(async () =>
                {
                    IEnumerable<ISearchResultModel> models = this.model.CreateChildren();
                    foreach (ISearchResultModel model in models)
                    {
                        await this.Dispatcher.RunIdleAsync(new IdleDispatchedHandler((args) =>
                        {
                            this.AddChild(model);
                        }));
                    }
                });
                this.LoadingChildren = false;
            }
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

                this.ProcessSelectionChanged(this, new SearchResultSelectedChangedEventArgs(this, list, listItem.IsSelected));
            }
        }

        private void TopBar_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.DoubleClick?.Invoke(this, new SearchResultEventArgs(this));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void UserControl_LoadedAsync(object sender, RoutedEventArgs e)
        {
            if (this.ShowImage)
            {
                this.ImageImage.Source = await this.model.LoadImageAsync();
            }
        }

        private void TopBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // this.ExpanderToggleState = !this.ExpanderToggleState;
        }

        private void ChildrenListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchResult clickedSearchResult;
            clickedSearchResult = e.AddedItems.FirstOrDefault() as SearchResult;
            if (clickedSearchResult != null)
            {
                clickedSearchResult.ExpanderToggleState = true;
            }
            else
            {
                clickedSearchResult = e.RemovedItems.FirstOrDefault() as SearchResult;
                if (clickedSearchResult != null)
                {
                    clickedSearchResult.ExpanderToggleState = false;
                }
            }

        }
    }
}
