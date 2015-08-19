using Windows.UI.Xaml.Controls;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class ArtistsPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ArtistsPageViewModel(ILibraryService libraryService, INavigationService navigationService)
        {
            _navigationService = navigationService;
            LibraryService = libraryService;

            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);

            if (IsInDesignMode)
                LibraryService.Load();
        }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }
        public ILibraryService LibraryService { get; }

        private void ArtistClickExecute(ItemClickEventArgs e)
        {
            var artist = (Artist) e.ClickedItem;
            _navigationService.Navigate(typeof (ArtistPage), artist.Name);
        }
    }
}