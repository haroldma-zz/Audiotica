using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class NowPlayingPageViewModel : ViewModelBase
    {
        public NowPlayingPageViewModel(IPlayerService playerService)
        {
            PlayerService = playerService;
        }

        public IPlayerService PlayerService { get; }
    }
}