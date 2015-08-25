using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.Enums;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class AlbumsPage
    {
        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as AlbumsPageViewModel;
        }

        public AlbumsPageViewModel ViewModel { get; }

        private void LibraryHeader_OnCurrentSortChanged(object sender, ListBoxItem item)
        {
            if (!(item?.Tag is AlbumSort) || ViewModel == null) return;
            var sort = (AlbumSort) item.Tag;
            ViewModel.ChangeSort(sort);
            Bindings.Update();
        }
    }
}