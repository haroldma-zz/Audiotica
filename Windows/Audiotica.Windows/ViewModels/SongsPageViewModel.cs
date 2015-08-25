using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
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

            var defaultSort = _settingsUtility.Read(ApplicationSettingsConstants.SongSort, TrackSort.DateAdded, SettingsStrategy.Roam);
            DefaultSort = SortItems.IndexOf(SortItems.FirstOrDefault(p => (TrackSort) p.Tag == defaultSort));
            ChangeSort(defaultSort);
        }

        public int DefaultSort { get; }

        public List<ListBoxItem> SortItems { get; }

        public ILibraryService LibraryService { get; set; }

        public bool IsGrouped { get; private set; }

        public object TracksCollection { get; private set; }

        public void ChangeSort(TrackSort sort)
        {
            _settingsUtility.Write(ApplicationSettingsConstants.SongSort, sort, SettingsStrategy.Roam);
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