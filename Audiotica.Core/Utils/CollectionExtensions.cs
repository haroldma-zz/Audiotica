#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace Audiotica.Core.Utils
{
    public static class CollectionExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> observable, Comparison<T> comparison)
        {
            var sorted = observable.ToList();
            sorted.Sort(comparison);

            var ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    var t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                }
                else
                {
                    ptr++;
                }
            }
        }

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