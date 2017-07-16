using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laaud_UWP.Util
{
    static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string source, string match)
        {
            return source.IndexOf(match, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
