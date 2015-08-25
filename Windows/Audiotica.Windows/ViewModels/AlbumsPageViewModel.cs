using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class AlbumsPageViewModel : ViewModelBase
    {
        public AlbumsPageViewModel(ILibraryService libraryService, ILibraryCollectionService libraryCollectionService)
        {
            LibraryService = libraryService;

            AlbumsCollection = libraryCollectionService.AlbumsByTitle;
        }

        public ILibraryService LibraryService { get; }
        public object AlbumsCollection { get; set; }
    }
}