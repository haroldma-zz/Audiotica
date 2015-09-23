using System.Linq;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class PlayerBarViewModel : ViewModelBase
    {
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly IPlayerService _playerService;
        private QueueTrack _currentQueueTrack;

        public PlayerBarViewModel(IPlayerService playerService, IDispatcherUtility dispatcherUtility)
        {
            _playerService = playerService;
            _dispatcherUtility = dispatcherUtility;
            _playerService.TrackChanged += PlayerServiceOnTrackChanged;

            PlayPauseCommand = new Command(() => _playerService.PlayOrPause());
            NextCommand = new Command(() => _playerService.Next());
            PrevCommand = new Command(() => _playerService.Previous());
        }

        public Command PrevCommand { get; }

        public Command NextCommand { get; }

        public Command PlayPauseCommand { get; }

        public QueueTrack CurrentQueueTrack
        {
            get { return _currentQueueTrack; }
            set { Set(ref _currentQueueTrack, value); }
        }

        private void PlayerServiceOnTrackChanged(object sender, string s)
        {
            CurrentQueueTrack =
                _playerService.PlaybackQueue.FirstOrDefault(queueTrack => queueTrack.Id == s);
        }
    }
}