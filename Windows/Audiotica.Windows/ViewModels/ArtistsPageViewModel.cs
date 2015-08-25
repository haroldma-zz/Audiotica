using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
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
        private OptimizedObservableCollection<AlphaKeyGroup> _artistsCollection;

        public ArtistsPageViewModel(ILibraryCollectionService libraryCollectionService,
            ILibraryService libraryService,
            INavigationService navigationService)
        {
            LibraryService = libraryService;
            _libraryCollectionService = libraryCollectionService;
            _navigationService = navigationService;

            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);

            SortItems =
                Enum.GetValues(typeof (ArtistSort))
                    .Cast<ArtistSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();
            ChangeSort(ArtistSort.AtoZ);
        }

        public bool IsGrouped { get; set; }

        public ILibraryService LibraryService { get; set; }

        public List<ListBoxItem> SortItems { get; }

        public OptimizedObservableCollection<AlphaKeyGroup> ArtistsCollection
        {
            get { return _artistsCollection; }
            set { Set(ref _artistsCollection, value); }
        }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }

        public void ChangeSort(ArtistSort sort)
        {
            IsGrouped = sort != ArtistSort.AtoZ;

            switch (sort)
            {
                case ArtistSort.AtoZ:
                    ArtistsCollection = _libraryCollectionService.ArtistsByName;
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
    }
}