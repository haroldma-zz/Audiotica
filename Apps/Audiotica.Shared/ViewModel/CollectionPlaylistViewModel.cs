#region

using System;
using System.Collections.Specialized;
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
            if (Playlist != null)
            {
                Playlist.Songs.CollectionChanged -= SongsOnCollectionChanged;
            }
            Playlist = _service.Playlists.FirstOrDefault(p => p.Id == id);
            Playlist.Songs.CollectionChanged += SongsOnCollectionChanged;
        }

        private int _prevIndex = -1;
        private async void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _prevIndex != -1)
            {
                //an item was move using reorder
                await _service.MovePlaylistFromToAsync(Playlist, _prevIndex, e.NewStartingIndex);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
                _prevIndex = e.OldStartingIndex;
            else
                _prevIndex = -1;
        }

        private bool _currentlyPreparing;
        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = (PlaylistSong) (e.ClickedItem);
            var queueSong = _playlist.Songs.Select(p => p.Song).ToList();
            await CollectionHelper.PlaySongsAsync(song.Song, queueSong);
        }
    }
}