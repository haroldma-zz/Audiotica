using System;
using System.Collections.Generic;

namespace Audiotica.Windows.Engine.Utils
{
    public static class LinqUtils
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action?.Invoke(item);
            }
        }
    }
}
