using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class SongsPageViewModel : ViewModelBase
    {
        public SongsPageViewModel(ILibraryService libraryService)
        {
            LibraryService = libraryService;
            if (IsInDesignMode)
                LibraryService.Load();
        }

        public ILibraryService LibraryService { get; }
    }
}