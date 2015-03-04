using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;

namespace Audiotica.ViewModel
{
    public class MostPlayedViewModel : ViewModelBase
    {
        private readonly ICollectionService _collectionService;
        private List<Song> _mostPlayed;

        public MostPlayedViewModel(ICollectionService collectionService)
        {
            _collectionService = collectionService;

            _collectionService.Songs.CollectionChanged += Songs_CollectionChanged;
            Songs_CollectionChanged(null, null);
        }

        public List<Song> MostPlayed
        {
            get { return _mostPlayed; }
            set { Set(ref _mostPlayed, value); }
        }

        private void Songs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _collectionService.Songs.CollectionChanged -= Songs_CollectionChanged;

            if (_collectionService.Songs.Count < 4)
            {
                _collectionService.Songs.CollectionChanged += Songs_CollectionChanged;
            }

            CalculateMostPlayed();
        }

        private void CalculateMostPlayed()
        {
            MostPlayed =
               _collectionService.Songs.Where(p => p.PlayCount != 0 && (DateTime.Now - p.LastPlayed).TotalDays < 8)
                   .OrderByDescending(p => p.PlayCount)
                   .Take(4)
                   .ToList();

            if (MostPlayed.Count < 4)
            {
                App.Locator.AudioPlayerHelper.TrackChanged += AudioPlayerHelperOnTrackChanged;
            }
        }

        private void AudioPlayerHelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            App.Locator.AudioPlayerHelper.TrackChanged -= AudioPlayerHelperOnTrackChanged;
            CalculateMostPlayed();
        }
    }
}