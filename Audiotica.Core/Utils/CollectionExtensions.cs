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
        private static readonly Random Random = new Random();

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
            var arr = list.ToArray();
            Shuffle(arr);
            return arr;
        }

        private static void Shuffle<T>(T[] array)
        {
            var n = array.Length;
            for (var i = 0; i < n; i++)
            {
                // NextDouble returns a random number between 0 and 1.
                // ... It is equivalent to Math.random() in Java.
                var r = i + (int) (Random.NextDouble()*(n - i));
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
    }
}