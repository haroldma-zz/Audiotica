using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class ArtistsPageViewModel : ViewModelBase
    {
        public ArtistsPageViewModel(ILibraryService libraryService)
        {
            LibraryService = libraryService;
            if (IsInDesignMode)
                LibraryService.Load();
        }

        public ILibraryService LibraryService { get; }
    }
}