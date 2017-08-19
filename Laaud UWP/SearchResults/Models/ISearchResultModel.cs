using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Laaud_UWP.SearchResults.Models
{
    public interface ISearchResultModel
    {
        int Id { get; }

        string Title { get; }

        bool Favorite { get; set; }

        bool HasChildren { get; }

        IEnumerable<ISearchResultModel> CreateChildren();

        bool HasImage { get; }

        Task<ImageSource> LoadImageAsync();
    }
}
