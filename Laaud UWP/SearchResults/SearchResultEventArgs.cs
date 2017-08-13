using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.SearchResults
{
    public class SearchResultEventArgs : EventArgs
    {
        public SearchResult ChangedObject { get; }

        public SearchResultEventArgs(SearchResult changedObject)
        {
            this.ChangedObject = changedObject;
        }
    }
}
