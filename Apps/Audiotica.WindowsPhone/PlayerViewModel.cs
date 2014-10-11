#region

using System;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace Audiotica
{
    public class PlayerViewModel : ObservableObject
    {
        private readonly AudioPlayerHelper _helper;
        private readonly RelayCommand _nextRelayCommand;
        private readonly RelayCommand _playPauseRelayCommand;
        private readonly RelayCommand _prevRelayCommand;
        private readonly ICollectionService _service;
        private Song _currentSong;
        private bool _isLoading;
        private IconElement _playPauseIcon;

        public PlayerViewModel(AudioPlayerHelper helper, ICollectionService service)
        {
            _helper = helper;
            _service = service;
            helper.TrackChanged += HelperOnTrackChanged;
            helper.PlaybackStateChanged += HelperOnPlaybackStateChanged;
            helper.Shutdown += HelperOnShutdown;

            _nextRelayCommand = new RelayCommand(NextSong);
            _prevRelayCommand = new RelayCommand(PrevSong);
            _playPauseRelayCommand = new RelayCommand(PlayPauseToggle);
        }

        public Song CurrentSong
        {
            get { return _currentSong; }
            set { Set(ref _currentSong, value); }
        }

        public IconElement PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { Set(ref _playPauseIcon, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public RelayCommand NextRelayCommand
        {
            get { return _nextRelayCommand; }
        }

        public RelayCommand PrevRelayCommand
        {
            get { return _prevRelayCommand; }
        }

        public RelayCommand PlayPauseRelayCommand
        {
            get { return _playPauseRelayCommand; }
        }

        private bool IsViewVisible()
        {
            return App.RootFrame.SelectedPanelIndex == 1;
        }

        private void HelperOnShutdown(object sender, EventArgs eventArgs)
        {
            CurrentSong = null;
        }

        private void HelperOnPlaybackStateChanged(object sender, PlaybackStateEventArgs playbackStateEventArgs)
        {
            IsLoading = false;
            switch (playbackStateEventArgs.State)
            {
                case MediaPlayerState.Paused:
                    PlayPauseIcon = new SymbolIcon(Symbol.Play);
                    break;
                default:
                    PlayPauseIcon = new SymbolIcon(Symbol.Pause);
                    break;
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    IsLoading = true;
                    break;
            }
        }

        private void HelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            if (BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed &&
                BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
            {
                var id = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);
                CurrentSong = _service.Songs.FirstOrDefault(p => p.Id == id);
            }
            else
            {
                CurrentSong = null;
            }
        }

        private void PlayPauseToggle()
        {
            _helper.PlayPauseToggle();
        }

        private void PrevSong()
        {
            _helper.PrevSong();
        }

        private void NextSong()
        {
            _helper.NextSong();
        }
    }
}