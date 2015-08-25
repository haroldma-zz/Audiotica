using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;

namespace Audiotica.Windows.Tools
{
    public class AlphaKeyGroup : OptimizedObservableCollection<object>
    {
        /// <summary>
        ///     The delegate that is used to get the key information.
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The key value to use for this object</returns>
        public delegate string GetKeyDelegate(object item);

        /// <summary>
        ///     Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public AlphaKeyGroup(string key)
        {
            Key = key.ToUpper();
        }

        /// <summary>
        ///     The Key of this group.
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     The Order Key of this group.
        /// </summary>
        public GetKeyDelegate OrderKey { get; set; }

        /// <summary>
        ///     Create a list of AlphaGroup with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="slg">The </param>
        /// <returns>Theitems source for a LongListSelector</returns>
        public static List<AlphaKeyGroup> CreateGroups(CharacterGroupings slg)
        {
            return
                (from key in slg
                    where string.IsNullOrWhiteSpace(key.Label) == false
                    select new AlphaKeyGroup(key.Label)).ToList();
        }

        /// <summary>
        ///     Create a list of AlphaGroup with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="items">The items to place in the groups.</param>
        /// <param name="ci">The CultureInfo to group and sort by.</param>
        /// <param name="getKey">A delegate to get the key from an item.</param>
        /// <param name="sort">Will sort the data if true.</param>
        /// <returns>An items source for a LongListSelector</returns>
        public static OptimizedObservableCollection<AlphaKeyGroup> CreateGroups(IEnumerable<object> items, CultureInfo ci,
            GetKeyDelegate getKey,
            bool sort = true)
        {
            var slg = new CharacterGroupings();
            var list = CreateGroups(slg);

            foreach (var item in items)
            {
                var lookUp = getKey(item);

                if (string.IsNullOrEmpty(lookUp))
                    continue;

                var index = slg.Lookup(lookUp);
                if (string.IsNullOrEmpty(index) == false)
                {
                    list.Find(a => a.Key.EqualsIgnoreCase(index)).Items.Add(item);
                }
            }

            if (sort)
            {
                foreach (var group in list)
                {
                    group.OrderKey = getKey;
                    var asList = group.ToList();
                    asList.Sort((x, y) => string.Compare(getKey(x), getKey(y), StringComparison.Ordinal));
                    group.SwitchTo(asList);
                }
            }

            return new OptimizedObservableCollection<AlphaKeyGroup>(list);
        }
    }
}