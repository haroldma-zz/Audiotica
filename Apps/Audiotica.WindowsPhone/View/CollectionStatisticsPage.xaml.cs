#region

using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionStatisticsPage
    {
        public CollectionStatisticsPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(Windows.UI.Xaml.Navigation.NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            (DataContext as CollectionStatisticsViewModel).UpdateData();
        }
    }
}