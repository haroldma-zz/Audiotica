using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class AlbumsPageViewModel : ViewModelBase
    {
        public AlbumsPageViewModel(ILibraryService libraryService)
        {
            LibraryService = libraryService;
        }

        public ILibraryService LibraryService { get; }
    }
}