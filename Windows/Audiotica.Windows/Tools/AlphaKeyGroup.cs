using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Windows.UI.Xaml;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;

namespace Audiotica.Windows.Tools
{
    public class AlphaKeyGroup : OptimizedObservableCollection<object>
    {
        private GridLength _gridLeftLength;
        private GridLength _gridRightLength;

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

        public GridLength GridLeftLength
        {
            get { return _gridLeftLength; }
            set
            {
                _gridLeftLength = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GridLeftLength"));
            }
        }

        public GridLength GridRightLength
        {
            get { return _gridRightLength; }
            set
            {
                _gridRightLength = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GridRightLength"));
            }
        }

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

            var max = list.Select(p => p.Count).Max();

            if (sort)
            {
                foreach (var group in list)
                {
                    group.OrderKey = getKey;
                    var percent = group.Count > 1 ? Math.Log(group.Count, max) : Math.Log(2, max) / 2;
                    percent = Math.Max(.1, percent);
                    if (double.IsNaN(percent))
                        percent = 0.1;
                    if (group.Count == 0)
                        percent = 0;

                    group.GridLeftLength = new GridLength(percent, GridUnitType.Star);
                    group.GridRightLength = new GridLength(1 - percent, GridUnitType.Star);

                    var asList = group.ToList();
                    asList.Sort((x, y) => string.Compare(getKey(x), getKey(y), StringComparison.Ordinal));
                    group.SwitchTo(asList);

                    group.CollectionChanged += (sender, args) =>
                    {
                        max = list.Select(p => p.Count).Max();
                        foreach (var other in list)
                        {
                            percent = other.Count > 1 ? Math.Log(other.Count, max) : Math.Log(2, max) / 2;
                            percent = Math.Max(.1, percent);
                            if (double.IsNaN(percent))
                                percent = 0.1;
                            if (other.Count == 0)
                                percent = 0;

                            other.GridLeftLength = new GridLength(percent, GridUnitType.Star);
                            other.GridRightLength = new GridLength(1 - percent, GridUnitType.Star);
                        }
                    };
                }
            }

            return new OptimizedObservableCollection<AlphaKeyGroup>(list);
        }
    }
}