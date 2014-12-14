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
    public class CollectionArtistViewModel : ViewModelBase
    {
        private readonly ICollectionService _service;
        private readonly AudioPlayerHelper _audioPlayer;
        private Artist _artist;
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;

        public CollectionArtistViewModel(ICollectionService service, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;
            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            MessengerInstance.Register<GenericMessage<long>>(this, "artist-coll-detail-id", ReceivedId);

            if (IsInDesignMode)
                SetArtist(0);
        }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        private void ReceivedId(GenericMessage<long> obj)
        {
            SetArtist(obj.Content);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;

            await _service.ClearQueueAsync();

            foreach (var queueSong in _artist.Songs)
            {
                await _service.AddToQueueAsync(queueSong);
            }

#if WINDOWS_PHONE_APP
            _audioPlayer.PlaySong(song);
#endif
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickCommand; }
        }

        private void SetArtist(long id)
        {
            Artist = _service.Artists.FirstOrDefault(p => p.Id == id);
        }
    }
}