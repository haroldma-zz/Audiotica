using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Extensions;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SongsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;

        public SongsPageViewModel(ILibraryCollectionService libraryCollectionService, ILibraryService libraryService)
        {
            _libraryCollectionService = libraryCollectionService;
            LibraryService = libraryService;

            SortItems =
                Enum.GetValues(typeof (TrackSort))
                    .Cast<TrackSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();

            ChangeSort(TrackSort.DateAdded);
        }

        public List<ListBoxItem> SortItems { get; }

        public ILibraryService LibraryService { get; set; }

        public bool IsGrouped { get; private set; }

        public object TracksCollection { get; private set; }

        public void ChangeSort(TrackSort sort)
        {
            IsGrouped = sort != TrackSort.DateAdded;

            switch (sort)
            {
                case TrackSort.AtoZ:
                    TracksCollection = _libraryCollectionService.TracksByTitle;
                    break;
                case TrackSort.DateAdded:
                    TracksCollection = _libraryCollectionService.TracksByDateAdded;
                    break;
                case TrackSort.Artist:
                    TracksCollection = _libraryCollectionService.TracksByArtist;
                    break;
                case TrackSort.Album:
                    TracksCollection = _libraryCollectionService.TracksByAlbum;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }
    }
}