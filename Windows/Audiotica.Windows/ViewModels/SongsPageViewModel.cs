using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Enums;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SongsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;

        private bool _isGrouped;
        private object _tracksCollection;

        public SongsPageViewModel(ILibraryCollectionService libraryCollectionService, ILibraryService libraryService)
        {
            _libraryCollectionService = libraryCollectionService;
            LibraryService = libraryService;

            ChangeSort(TrackSort.DateAdded);
        }

        public ILibraryService LibraryService { get; set; }

        public bool IsGrouped
        {
            get { return _isGrouped; }
            set { Set(ref _isGrouped, value); }
        }

        public object TracksCollection
        {
            get { return _tracksCollection; }
            set { Set(ref _tracksCollection, value); }
        }

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