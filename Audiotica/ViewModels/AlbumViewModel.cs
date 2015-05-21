using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.ViewModels
{
    internal class AlbumViewModel : NavigatableViewModel
    {
        public AlbumViewModel()
        {
            // TODO
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            // Set Album from parameter or state
        }

        public override void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
            // Save the page album in the state
        }
    }
}