using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.SearchResult
{
    public class SearchResultFavoriteChangedEventArgs : SearchResultEventArgs
    {
        public bool NewIsFavorite { get; }

        public SearchResultFavoriteChangedEventArgs(SearchResult changedObject, bool newIsFavoriteValue) : base(changedObject)
        {
            this.NewIsFavorite = newIsFavoriteValue;
        }
    }
}
