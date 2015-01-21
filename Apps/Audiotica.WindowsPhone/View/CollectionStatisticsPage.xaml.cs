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

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);
            (DataContext as CollectionStatisticsViewModel).UpdateData();
        }
    }
}