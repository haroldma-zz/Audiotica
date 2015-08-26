using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SongsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;
        private readonly ISettingsUtility _settingsUtility;
        private int _selectedIndex;
        private double _verticalOffset;
        private CollectionViewSource _viewSource;

        public SongsPageViewModel(ILibraryCollectionService libraryCollectionService, ILibraryService libraryService,
            ISettingsUtility settingsUtility)
        {
            _libraryCollectionService = libraryCollectionService;
            _settingsUtility = settingsUtility;
            LibraryService = libraryService;

            SortItems =
                Enum.GetValues(typeof (TrackSort))
                    .Cast<TrackSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();
            SortChangedCommand = new Command<ListBoxItem>(SortChangedExecute);

            var defaultSort = _settingsUtility.Read(ApplicationSettingsConstants.SongSort, TrackSort.DateAdded,
                SettingsStrategy.Roam);
            DefaultSort = SortItems.IndexOf(SortItems.FirstOrDefault(p => (TrackSort) p.Tag == defaultSort));
            ChangeSort(defaultSort);
        }

        public Command<ListBoxItem> SortChangedCommand { get; }

        public int DefaultSort { get; }

        public List<ListBoxItem> SortItems { get; }

        public ILibraryService LibraryService { get; set; }

        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { Set(ref _viewSource, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { Set(ref _selectedIndex, value); }
        }

        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set { Set(ref _verticalOffset, value); }
        }

        private void SortChangedExecute(ListBoxItem item)
        {
            if (!(item?.Tag is TrackSort)) return;
            var sort = (TrackSort) item.Tag;
            ChangeSort(sort);
        }

        public void ChangeSort(TrackSort sort)
        {
            _settingsUtility.Write(ApplicationSettingsConstants.SongSort, sort, SettingsStrategy.Roam);
            ViewSource = new CollectionViewSource {IsSourceGrouped = sort != TrackSort.DateAdded};

            switch (sort)
            {
                case TrackSort.AtoZ:
                    ViewSource.Source = _libraryCollectionService.TracksByTitle;
                    break;
                case TrackSort.DateAdded:
                    ViewSource.Source = _libraryCollectionService.TracksByDateAdded;
                    break;
                case TrackSort.Artist:
                    ViewSource.Source = _libraryCollectionService.TracksByArtist;
                    break;
                case TrackSort.Album:
                    ViewSource.Source = _libraryCollectionService.TracksByAlbum;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            if (state.ContainsKey("VerticalOffset"))
            {
                VerticalOffset = (double) state["VerticalOffset"];
                SelectedIndex = int.Parse(state["SelectedIndex"].ToString());
            }
        }

        public override void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
            state["VerticalOffset"] = VerticalOffset;
            state["SelectedIndex"] = SelectedIndex;
        }
    }
}