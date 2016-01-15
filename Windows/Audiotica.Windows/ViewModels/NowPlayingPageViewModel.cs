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

        public IPlayerService PlayerService { get; }
    }
}