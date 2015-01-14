#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endregion

namespace Audiotica
{
    public sealed partial class SongViewer
    {
        public SongViewer()
        {
            InitializeComponent();
        }

        private void AddToMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.AddToClickCommand.Execute(DataContext);
        }

        private void Picker_OnItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            App.Locator.Collection.Commands.ItemPickedCommand.Execute(args.AddedItems[0]);
        }

        private void DeleteMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DeleteClickCommand.Execute(DataContext);
        }

        private void DownloadButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DownloadClickCommand.Execute(DataContext);
        }

        private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.CancelClickCommand.Execute(DataContext);
        }
    }
}