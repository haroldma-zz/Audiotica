#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Audiotica.Core.Utilities;
using MyToolkit.Collections;

#endregion

namespace Audiotica.Core.Common
{
    public class AlphaKeyGroup<T> : OptimizedObservableCollection<T>
    {
        /// <summary>
        ///     The delegate that is used to get the key information.
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The key value to use for this object</returns>
        public delegate string GetKeyDelegate(T item);

        /// <summary>
        ///     Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public AlphaKeyGroup(string key)
        {
            Key = key.ToLower();
        }

        /// <summary>
        ///     The Key of this group.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        ///     The Order Key of this group.
        /// </summary>
        public GetKeyDelegate OrderKey { get; private set; }

        /// <summary>
        ///     Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="slg">The </param>
        /// <returns>Theitems source for a LongListSelector</returns>
        public static List<AlphaKeyGroup<T>> CreateGroups(CharacterGroupings slg)
        {
            var list = new List<AlphaKeyGroup<T>>();

            foreach (CharacterGrouping key in slg)
            {
                if (string.IsNullOrWhiteSpace(key.Label) == false)
                    list.Add(new AlphaKeyGroup<T>(key.Label));
            }

            return list;
        }

        /// <summary>
        ///     Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="items">The items to place in the groups.</param>
        /// <param name="ci">The CultureInfo to group and sort by.</param>
        /// <param name="getKey">A delegate to get the key from an item.</param>
        /// <param name="sort">Will sort the data if true.</param>
        /// <returns>An items source for a LongListSelector</returns>
        public static ObservableCollection<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, CultureInfo ci, GetKeyDelegate getKey,
            bool sort)
        {
            var slg = new CharacterGroupings();
            var list = CreateGroups(slg);

            foreach (T item in items)
            {
                var index = "";
                index = slg.Lookup(getKey(item));
                if (string.IsNullOrEmpty(index) == false)
                {
                    list.Find(a => a.Key == index).Items.Add(item);
                }
            }

            if (sort)
            {
                foreach (AlphaKeyGroup<T> group in list)
                {
                    group.OrderKey = getKey;
                    var asList = group.ToList();
                    asList.Sort((x, y) => String.Compare(getKey(x), getKey(y), StringComparison.Ordinal));
                    group.SwitchTo(asList);
                }
            }

            return new ObservableCollection<AlphaKeyGroup<T>>(list);
        }
    }
}