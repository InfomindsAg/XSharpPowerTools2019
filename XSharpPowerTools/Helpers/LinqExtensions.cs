using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpPowerTools.Helpers
{
    public static class LinqExtensions
    {
        public static async Task<bool> AnyAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> func)
        {
            foreach (var element in source)
            {
                if (await func(element))
                    return true;
            }
            return false;
        }
    }
}
