using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class ArtistsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;
        private readonly INavigationService _navigationService;
        private double _gridViewVerticalOffset;
        private double _listViewVerticalOffset;
        private CollectionViewSource _viewSource;

        public ArtistsPageViewModel(ILibraryCollectionService libraryCollectionService,
            ILibraryService libraryService,
            INavigationService navigationService)
        {
            LibraryService = libraryService;
            _libraryCollectionService = libraryCollectionService;
            _navigationService = navigationService;

            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);
            SortChangedCommand = new Command<ListBoxItem>(SortChangedExecute);

            SortItems =
                Enum.GetValues(typeof (ArtistSort))
                    .Cast<ArtistSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();
            ChangeSort(ArtistSort.AtoZ);
        }

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