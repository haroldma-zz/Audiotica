using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SongsPageViewModel : ViewModelBase
    {
        public SongsPageViewModel(ILibraryCollectionService libraryCollectionService, ILibraryService libraryService)
        {
            LibraryService = libraryService;
            TracksCollection = libraryCollectionService.TracksByTitle;
        }

        public ILibraryService LibraryService { get; set; }

        public object TracksCollection { get; set; }
    }
}