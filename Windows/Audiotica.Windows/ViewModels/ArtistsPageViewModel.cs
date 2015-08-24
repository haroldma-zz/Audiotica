using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class ArtistsPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private OptimizedObservableCollection<AlphaKeyGroup> _artistsCollection;

        public ArtistsPageViewModel(ILibraryCollectionService libraryCollectionService,
            INavigationService navigationService)
        {
            _navigationService = navigationService;

            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);

            ArtistsCollection = libraryCollectionService.ArtistsByName;
        }

        public OptimizedObservableCollection<AlphaKeyGroup> ArtistsCollection
        {
            get { return _artistsCollection; }
            set { Set(ref _artistsCollection, value); }
        }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }

        private void ArtistClickExecute(ItemClickEventArgs e)
        {
            var artist = (Artist) e.ClickedItem;
            _navigationService.Navigate(typeof (ArtistPage), artist.Name);
        }
    }
}