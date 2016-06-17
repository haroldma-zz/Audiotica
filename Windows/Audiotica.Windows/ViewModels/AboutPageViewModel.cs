using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.Engine.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class AboutPageViewModel : ViewModelBase
    {
        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            AnalyticService.TrackPageView("About");
        }
    }
}