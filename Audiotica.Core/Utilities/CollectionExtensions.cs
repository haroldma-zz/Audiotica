#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Audiotica.Core.Utilities
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            var shuffleList = new T[n];
            list.CopyTo(shuffleList, 0);

            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = shuffleList[k];
                shuffleList[k] = shuffleList[n];
                shuffleList[n] = value;
            }

            return shuffleList;
        }
    }
}