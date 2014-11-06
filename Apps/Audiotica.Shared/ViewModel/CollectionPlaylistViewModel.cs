#region

using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionPlaylistViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;
        private readonly ICollectionService _service;
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;
        private Playlist _playlist;

        public CollectionPlaylistViewModel(ICollectionService service, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;
            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            MessengerInstance.Register<GenericMessage<long>>(this, "playlist-coll-detail-id", ReceivedId);

            if (IsInDesignMode)
                SetPlaylist(0);
        }

        public Playlist Playlist
        {
            get { return _playlist; }
            set { Set(ref _playlist, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }

        private void ReceivedId(GenericMessage<long> obj)
        {
            SetPlaylist(obj.Content);
        }

        public void SetPlaylist(long id)
        {
            Playlist = _service.Playlists.FirstOrDefault(p => p.Id == id);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = (PlaylistSong) (e.ClickedItem);

            await _service.ClearQueueAsync();

            foreach (var playlistSong in _playlist.Songs)
            {
                await _service.AddToQueueAsync(playlistSong.Song);
            }

#if WINDOWS_PHONE_APP
            _audioPlayer.PlaySong(song.SongId);
#endif
        }
    }
}