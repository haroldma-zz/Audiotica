using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;

namespace Audiotica.ViewModel
{
    public class RecentlyAddedViewModel : ViewModelBase
    {
        private readonly ICollectionService _collectionService;
        private List<Song> _recentlyAdded;

        public RecentlyAddedViewModel(ICollectionService collectionService)
        {
            _collectionService = collectionService;

            _collectionService.Songs.CollectionChanged += Songs_CollectionChanged;
            Songs_CollectionChanged(null, null);
        }

        public List<Song> RecentlyAdded
        {
            get { return _recentlyAdded; }
            set { Set(ref _recentlyAdded, value); }
        }

        private void Songs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var recently = _collectionService.Songs.OrderByDescending(p => p.Id).Take(4).ToList();

            if (RecentlyAdded == null || !recently.SequenceEqual(RecentlyAdded))
                RecentlyAdded = recently;
        }
    }
}