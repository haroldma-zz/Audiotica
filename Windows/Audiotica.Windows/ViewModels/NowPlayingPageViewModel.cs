using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.ViewModels
{
    public class NowPlayingPageViewModel : ViewModelBase
    {
        public NowPlayingPageViewModel(IPlayerService playerService)
        {
            PlayerService = playerService;
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            AnalyticService.TrackPageView("Now Playing");
        }

        public IPlayerService PlayerService { get; }
    }
}