using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.Enums;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class SongsPage
    {
        public SongsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as SongsPageViewModel;
        }

        public SongsPageViewModel ViewModel { get; set; }
        
        private void LibraryHeader_OnCurrentSortChanged(object sender, ListBoxItem item)
        {
            if (!(item?.Tag is TrackSort) || ViewModel == null) return;
            var sort = (TrackSort)item.Tag;
            ViewModel.ChangeSort(sort);
            Bindings.Update();
        }
    }
}