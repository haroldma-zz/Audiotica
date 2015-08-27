using System;
using System.Collections.Generic;
using System.Linq;

namespace Audiotica.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            var array = enumerable.ToArray();
            for (var i = 0; i < array.Length; i++)
                action(array[i], i);
        }

        public static int GetMostUsedOccurrenceWhileIgnoringZero<T>(this IEnumerable<T> list, Func<T, int> func)
        {
            var ocurrences = list.GetMostUsedList(func);
            if (ocurrences.Count > 1)
                ocurrences = ocurrences.Where(p => p != 0).ToList();

            return ocurrences.FirstOrDefault();
        }

        public static TReturn GetMostUsedOccurrence<T, TReturn>(this IEnumerable<T> list, Func<T, TReturn> func)
        {
            return list.GetMostUsedList(func).FirstOrDefault();
        }

        public static List<TReturn> GetMostUsedList<T, TReturn>(this IEnumerable<T> list, Func<T, TReturn> func)
        {
            var returnTypeDictionary = list.GetMostUsedDict(func);
            return returnTypeDictionary.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
        }

        public static Dictionary<TReturn, int> GetMostUsedDict<T, TReturn>(this IEnumerable<T> list,
            Func<T, TReturn> func)
        {
            var valueDictionary = new Dictionary<TReturn, int>();

            foreach (var funcValue in list.Select(func))
            {
                int value;
                if (valueDictionary.TryGetValue(funcValue, out value))
                {
                    valueDictionary[funcValue] = ++value;
                }
                else
                {
                    valueDictionary.Add(funcValue, 1);
                }
            }

            return valueDictionary;
        }
    }
}