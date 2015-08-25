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
    public class AlbumsPageViewModel : ViewModelBase
    {
        private readonly ILibraryCollectionService _libraryCollectionService;

        public AlbumsPageViewModel(ILibraryService libraryService, ILibraryCollectionService libraryCollectionService)
        {
            _libraryCollectionService = libraryCollectionService;
            LibraryService = libraryService;

            SortItems =
                Enum.GetValues(typeof (AlbumSort))
                    .Cast<AlbumSort>()
                    .Select(sort => new ListBoxItem {Content = sort.GetEnumText(), Tag = sort})
                    .ToList();

            ChangeSort(AlbumSort.DateAdded);
        }

        public List<ListBoxItem> SortItems { get; set; }

        public ILibraryService LibraryService { get; }

        public object AlbumsCollection { get; private set; }

        public bool IsGrouped { get; private set; }

        public void ChangeSort(AlbumSort sort)
        {
            IsGrouped = sort != AlbumSort.DateAdded;

            switch (sort)
            {
                case AlbumSort.AtoZ:
                    AlbumsCollection = _libraryCollectionService.AlbumsByTitle;
                    break;
                case AlbumSort.DateAdded:
                    AlbumsCollection = _libraryCollectionService.AlbumsByDateAdded;
                    break;
                case AlbumSort.Artist:
                    AlbumsCollection = _libraryCollectionService.AlbumsByArtist;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }
        }
    }
}