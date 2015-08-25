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

        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.FirstOrDefault() as ListBoxItem;
            if (item?.Content == null || ViewModel == null) return;
            var sort = (TrackSort) Enum.Parse(typeof(TrackSort), item?.Content?.ToString().Replace(" ", ""));
            ViewModel.ChangeSort(sort);
            Bindings.Update();
        }
    }
}