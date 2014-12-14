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
    public class CollectionAlbumViewModel : ViewModelBase
    {
        private readonly ICollectionService _service;
        private readonly AudioPlayerHelper _audioPlayer;
        private Album _album;
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;

        public CollectionAlbumViewModel(ICollectionService service, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;
            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            MessengerInstance.Register<GenericMessage<long>>(this, "album-coll-detail-id", ReceivedId);

            if (IsInDesignMode)
                SetAlbum(0);
        }

        private void ReceivedId(GenericMessage<long> obj)
        {
            SetAlbum(obj.Content);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;

            await _service.ClearQueueAsync();

            foreach (var queueSong in _album.Songs)
            {
                await _service.AddToQueueAsync(queueSong);
            }

#if WINDOWS_PHONE_APP
            _audioPlayer.PlaySong(song);
#endif
        }

        public Album Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }

        public void SetAlbum(long id)
        {
            Album = _service.Albums.FirstOrDefault(p => p.Id == id);
        }
    }
}