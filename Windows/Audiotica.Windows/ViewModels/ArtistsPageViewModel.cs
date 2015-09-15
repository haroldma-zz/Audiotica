using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class ArtistsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private double _gridViewVerticalOffset;
        private bool? _isSelectMode = false;
        private double _listViewVerticalOffset;
        private CollectionViewSource _viewSource;

        public ArtistsPageViewModel(ILibraryCollectionService libraryCollectionService,
            ILibraryService libraryService, IPlayerService playerService,
            INavigationService navigationService)
        {
            LibraryService = libraryService;
            _libraryCollectionService = libraryCollectionService;
            _playerService = playerService;
            _navigationService = navigationService;

            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);
            SortChangedCommand = new Command<ListBoxItem>(SortChangedExecute);
            ShuffleAllCommand = new Command(ShuffleAllExecute);

            SortItems =
                Enum.GetValues(typeof (ArtistSort))
                    .Cast<ArtistSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();
            ChangeSort(ArtistSort.AtoZ);
        }

        public Command ShuffleAllCommand { get; }

        public Command<ListBoxItem> SortChangedCommand { get; }

        public ILibraryService LibraryService { get; set; }

        public List<ListBoxItem> SortItems { get; }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }

        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { Set(ref _viewSource, value); }
        }

        public double ListViewVerticalOffset
        {
            get { return _listViewVerticalOffset; }
            set { Set(ref _listViewVerticalOffset, value); }
        }

        public double GridViewVerticalOffset
        {
            get { return _gridViewVerticalOffset; }
            set { Set(ref _gridViewVerticalOffset, value); }
        }

        public bool? IsSelectMode
        {
            get { return _isSelectMode; }
            set { Set(ref _isSelectMode, value); }
        }

        private async void ShuffleAllExecute()
        {
            var playable = LibraryService.Tracks
                .Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                .ToList();
            if (!playable.Any()) return;

            var tracks = playable.Shuffle();
            await _playerService.NewQueueAsync(tracks);
        }

        private void SortChangedExecute(ListBoxItem item)
        {
            if (!(item?.Tag is ArtistSort)) return;
            var sort = (ArtistSort) item.Tag;
            ChangeSort(sort);
        }

        public void ChangeSort(ArtistSort sort)
        {
            ViewSource = new CollectionViewSource {IsSourceGrouped = true};

            switch (sort)
            {
                case ArtistSort.AtoZ:
                    ViewSource.Source = _libraryCollectionService.ArtistsByName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }

        private void ArtistClickExecute(ItemClickEventArgs e)
        {
            if (IsSelectMode == true) return;
            var artist = (Artist) e.ClickedItem;
            _navigationService.Navigate(typeof (ArtistPage), artist.Name);
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            if (state.ContainsKey("GridViewVerticalOffset"))
            {
                var gridOffset = (double) state["GridViewVerticalOffset"];
                var listOffset = (double) state["ListViewVerticalOffset"];
                GridViewVerticalOffset = gridOffset;
                ListViewVerticalOffset = listOffset;
            }
        }

        public override void OnSaveState(bool suspending, Dictionary<string, object> state)
        {
            state["GridViewVerticalOffset"] = GridViewVerticalOffset;
            state["ListViewVerticalOffset"] = ListViewVerticalOffset;
        }
    }
}