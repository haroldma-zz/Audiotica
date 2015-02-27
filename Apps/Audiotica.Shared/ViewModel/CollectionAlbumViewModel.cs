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
        private readonly CollectionCommandHelper _commands;
        private Album _album;
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;

        public CollectionAlbumViewModel(ICollectionService service, CollectionCommandHelper commands)
        {
            _service = service;
            _commands = commands;
            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            
            if (IsInDesignMode)
                SetAlbum(0);
            else MessengerInstance.Register<GenericMessage<int>>(this, "album-coll-detail-id", ReceivedId);
        }

        private void ReceivedId(GenericMessage<int> obj)
        {
            SetAlbum(obj.Content);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var queueSong = _album.Songs.ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
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

        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        public void SetAlbum(long id)
        {
            Album = _service.Albums.FirstOrDefault(p => p.Id == id);
        }
    }
}