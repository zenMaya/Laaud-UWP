using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Laaud_UWP.SearchResults
{
    public class SearchResultSelectedChangedEventArgs : SearchResultEventArgs
    {
        public ListView ParentList { get; }
        public bool IsSelected { get; }

        public SearchResultSelectedChangedEventArgs(SearchResult changedObject, ListView parentList, bool isSelected) : base(changedObject)
        {
            this.ParentList = parentList;
            this.IsSelected = isSelected;
        }
    }
}
