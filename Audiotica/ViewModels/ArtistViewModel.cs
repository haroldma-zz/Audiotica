using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.ViewModels
{
    internal class ArtistViewModel : NavigatableViewModel
    {
        public ArtistViewModel()
        {
            // TODO
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            // Set Artist from parameter or state
        }

        public override void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
            // Save the page artist in the state
        }
    }
}