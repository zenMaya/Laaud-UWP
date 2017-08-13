using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Util
{
    public static class ListExtensions
    {
        public static T GetNextAfter<T>(this List<T> list, T item)
        {
            int indexOfCurrent = list.IndexOf(item);
            int indexOfNext = indexOfCurrent + 1;

            if (indexOfNext >= list.Count)
            {
                indexOfNext = 0;
            }

            return list[indexOfNext];
        }
    }
}
