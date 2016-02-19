using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Engine.Navigation;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class AlbumsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly ISettingsUtility _settingsUtility;
        private double _gridViewVerticalOffset;
        private bool? _isSelectMode = false;
        private double _listViewVerticalOffset;
        private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
        private CollectionViewSource _viewSource;

        public AlbumsPageViewModel(
            ILibraryService libraryService,
            ILibraryCollectionService libraryCollectionService,
            IPlayerService playerService,
            ISettingsUtility settingsUtility,
            INavigationService navigationService)
        {
            _libraryCollectionService = libraryCollectionService;
            _playerService = playerService;
            _settingsUtility = settingsUtility;
            _navigationService = navigationService;
            LibraryService = libraryService;

            AlbumClickCommand = new DelegateCommand<ItemClickEventArgs>(AlbumClickExecute);
            SortChangedCommand = new DelegateCommand<ListBoxItem>(SortChangedExecute);
            ShuffleAllCommand = new DelegateCommand(ShuffleAllExecute);

            SortItems =
                Enum.GetValues(typeof (AlbumSort))
                    .Cast<AlbumSort>()
                    .Select(sort => new ListBoxItem { Content = sort.GetEnumText(), Tag = sort })
                    .ToList();

            var defaultSort = _settingsUtility.Read(ApplicationSettingsConstants.AlbumSort,
                AlbumSort.DateAdded,
                SettingsStrategy.Roam);
            DefaultSort = SortItems.IndexOf(SortItems.FirstOrDefault(p => (AlbumSort)p.Tag == defaultSort));
            ChangeSort(defaultSort);
        }

        public DelegateCommand<ItemClickEventArgs> AlbumClickCommand { get; }

        public int DefaultSort { get; }

        public double GridViewVerticalOffset
        {
            get
            {
                return _gridViewVerticalOffset;
            }
            set
            {
                Set(ref _gridViewVerticalOffset, value);
            }
        }

        public bool? IsSelectMode
        {
            get
            {
                return _isSelectMode;
            }
            set
            {
                Set(ref _isSelectMode, value);
            }
        }

        public ILibraryService LibraryService { get; }

        public double ListViewVerticalOffset
        {
            get
            {
                return _listViewVerticalOffset;
            }
            set
            {
                Set(ref _listViewVerticalOffset, value);
            }
        }

        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return _selectedItems;
            }
            set
            {
                Set(ref _selectedItems, value);
            }
        }

        public DelegateCommand ShuffleAllCommand { get; }

        public DelegateCommand<ListBoxItem> SortChangedCommand { get; }

        public List<ListBoxItem> SortItems { get; set; }

        public CollectionViewSource ViewSource
        {
            get
            {
                return _viewSource;
            }
            set
            {
                Set(ref _viewSource, value);
            }
        }

        public void ChangeSort(AlbumSort sort)
        {
            ViewSource = new CollectionViewSource { IsSourceGrouped = sort != AlbumSort.DateAdded };
            _settingsUtility.Write(ApplicationSettingsConstants.AlbumSort, sort, SettingsStrategy.Roam);

            switch (sort)
            {
                case AlbumSort.AtoZ:
                    ViewSource.Source = _libraryCollectionService.AlbumsByTitle;
                    break;
                case AlbumSort.DateAdded:
                    ViewSource.Source = _libraryCollectionService.AlbumsByDateAdded;
                    break;
                case AlbumSort.Artist:
                    ViewSource.Source = _libraryCollectionService.AlbumsByArtist;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey("GridViewVerticalOffset"))
            {
                var gridOffset = (double)state["GridViewVerticalOffset"];
                var listOffset = (double)state["ListViewVerticalOffset"];
                GridViewVerticalOffset = gridOffset;
                ListViewVerticalOffset = listOffset;
            }
        }

        public override void OnSaveState(IDictionary<string, object> state, bool suspending)
        {
            state["GridViewVerticalOffset"] = GridViewVerticalOffset;
            state["ListViewVerticalOffset"] = ListViewVerticalOffset;
        }

        private void AlbumClickExecute(ItemClickEventArgs e)
        {
            if (IsSelectMode == true)
            {
                return;
            }
            var album = (Album)e.ClickedItem;
            _navigationService.Navigate(typeof (AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name));
        }

        private async void ShuffleAllExecute()
        {
            var playable = LibraryService.Tracks
                .Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                .ToList();
            if (!playable.Any())
            {
                return;
            }

            var tracks = playable.Shuffle();
            await _playerService.NewQueueAsync(tracks);
        }

        private void SortChangedExecute(ListBoxItem item)
        {
            if (!(item?.Tag is AlbumSort))
            {
                return;
            }
            var sort = (AlbumSort)item.Tag;
            ChangeSort(sort);
        }
    }
}