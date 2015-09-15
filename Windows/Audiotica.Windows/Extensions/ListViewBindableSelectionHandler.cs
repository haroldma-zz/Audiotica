using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Helpers;

namespace Audiotica.Windows.Extensions
{
    /// <summary>
    ///     Handles synchronization of ListViewExtensions.BindableSelection to a ListView.
    /// </summary>
    public class ListViewBindableSelectionHandler
    {
        private readonly NotifyCollectionChangedEventHandler _handler;
        private ObservableCollection<object> _boundSelection;
        private ListViewBase _listView;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListViewBindableSelectionHandler" /> class.
        /// </summary>
        /// <param name="listView">The ListView.</param>
        /// <param name="boundSelection">The bound selection.</param>
        public ListViewBindableSelectionHandler(
            ListViewBase listView, ObservableCollection<object> boundSelection)
        {
            _handler = OnBoundSelectionChanged;
            Attach(listView, boundSelection);
        }

        private void Attach(ListViewBase listView, ObservableCollection<object> boundSelection)
        {
            _listView = listView;
            _listView.SelectionChanged += OnListViewSelectionChanged;
            _boundSelection = boundSelection;

            foreach (var item in _boundSelection.Where(item => !_listView.SelectedItems.Contains(item)))
            {
                _listView.SelectedItems.Add(item);
            }

            _boundSelection.CollectionChanged += OnBoundSelectionChanged;
        }

        private void OnListViewSelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems.Where(item => _boundSelection.Contains(item)))
            {
                _boundSelection.Remove(item);
            }

            foreach (var item in e.AddedItems.Where(item => !_boundSelection.Contains(item)))
            {
                _boundSelection.Add(item);
            }
        }

        private void OnBoundSelectionChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action ==
                NotifyCollectionChangedAction.Reset)
            {
                _listView.SelectedItems.Clear();

                foreach (var item in _boundSelection)
                {
                    if (!_listView.SelectedItems.Contains(item))
                    {
                        _listView.SelectedItems.Add(item);
                    }
                }

                return;
            }

            try
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.Cast<object>().Where(item => _listView.SelectedItems.Contains(item))
                        )
                    {
                        _listView.SelectedItems.Remove(item);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (
                        var item in e.NewItems.Cast<object>().Where(item => !_listView.SelectedItems.Contains(item)))
                    {
                        _listView.SelectedItems.Add(item);
                    }
                }
            }
            catch { }
        }

        internal void Detach()
        {
            _listView.SelectionChanged -= OnListViewSelectionChanged;
            _listView = null;
            _boundSelection.CollectionChanged -= _handler;
            _boundSelection = null;
        }
    }
}