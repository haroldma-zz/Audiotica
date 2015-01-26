using System;
using System.Collections.Specialized;
using Android.Content;
using Android.Views;
using Android.Widget;
using Audiotica.Core.Common;

namespace Audiotica.Android.Utilities
{
    public class CustomArrayAdapter<T> : ArrayAdapter<T>
    {
        private readonly OptimizedObservableCollection<T> _collection;
        private readonly Func<int, View, ViewGroup, View> _getView;

        public CustomArrayAdapter(Context context, int textViewResourceId, OptimizedObservableCollection<T> collection,
            Func<int, View, ViewGroup, View> getView)
            : base(context, textViewResourceId, collection)
        {
            _getView = getView;
            _collection = collection;
            collection.CollectionChanged += ObjectsOnCollectionChanged;
        }

        private void ObjectsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Insert((T) e.NewItems[0], e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    foreach (var item in _collection)
                    {
                        Add(item);
                    }
                    break;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return _getView(position, convertView, parent);
        }
    }
}