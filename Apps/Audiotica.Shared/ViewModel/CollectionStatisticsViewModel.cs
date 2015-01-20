#region

using System;
using System.Collections.Generic;
using System.Linq;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionStatisticsViewModel : ViewModelBase
    {
        private List<Artist> _topArtists;
        private List<Artist> _topArtistsToday;
        private int _totalHours;
        private int _totalMinutes;

        public CollectionStatisticsViewModel(ICollectionService service)
        {
            Service = service;
        }

        public ICollectionService Service { get; private set; }

        public int TotalHours
        {
            get { return _totalHours; }
            set { Set(ref _totalHours, value); }
        }

        public List<Artist> TopArtists
        {
            get { return _topArtists; }
            set { Set(ref _topArtists, value); }
        }

        public List<Artist> TopArtistsToday
        {
            get { return _topArtistsToday; }
            set { Set(ref _topArtistsToday, value); }
        }

        public int TotalMinutes
        {
            get { return _totalMinutes; }
            set { Set(ref _totalMinutes, value); }
        }

        public void UpdateData()
        {
            var total = Service.Songs.Select(p => p.Duration.TotalSeconds*p.PlayCount).Sum();
            var totalSpan = TimeSpan.FromSeconds(total);
            TotalHours = (int) totalSpan.TotalHours;
            TotalMinutes = (int) totalSpan.TotalMinutes;

            TopArtists = Service.Artists.OrderByDescending(p => p.Songs.Select(m => m.PlayCount).Sum())
                .Where(p => p.Songs.Select(m => m.PlayCount).Sum() != 0).Take(5).ToList();

            TopArtistsToday = Service.Artists
                .OrderByDescending(
                    p => p.Songs.Where(m => (DateTime.Now - m.LastPlayed).TotalDays < 1)
                        .Select(m => m.PlayCount).Sum())
                .Where(p => p.Songs.Select(m => m.PlayCount).Sum() != 0).Take(5).ToList();
        }
    }
}