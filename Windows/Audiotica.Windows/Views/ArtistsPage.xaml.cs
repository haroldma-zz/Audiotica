using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.Enums;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ArtistsPage
    {
        public ArtistsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as ArtistsPageViewModel;
        }

        public ArtistsPageViewModel ViewModel { get; }

        private void LibraryHeader_OnCurrentSortChanged(object sender, ListBoxItem item)
        {
            if (!(item?.Tag is ArtistSort) || ViewModel == null) return;
            var sort = (ArtistSort) item.Tag;
            ViewModel.ChangeSort(sort);
            Bindings.Update();
        }
    }
}