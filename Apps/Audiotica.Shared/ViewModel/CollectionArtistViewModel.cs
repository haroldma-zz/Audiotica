#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionArtistViewModel : ViewModelBase
    {
        private readonly ICollectionService _service;
        private readonly CollectionCommandHelper _commands;
        private readonly IScrobblerService _lastService;
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;
        private Artist _artist;
        private LastArtist _lastArtist;

        public CollectionArtistViewModel(ICollectionService service, CollectionCommandHelper commands, IScrobblerService lastService)
        {
            _service = service;
            _commands = commands;
            _lastService = lastService;
            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            MessengerInstance.Register<int>(this, "artist-coll-detail-id", ReceivedId);

            if (IsInDesignMode)
                SetArtist(0);
        }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public LastArtist LastArtist
        {
            get { return _lastArtist; }
            set { Set(ref _lastArtist, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickCommand
        {
            get { return _songClickCommand; }
        }

        public CollectionCommandHelper Commands
        {
            get { return _commands; }
        }

        private void ReceivedId(int id)
        {
            if (Artist != null && Artist.Id == id) return;

            Messenger.Default.Send(false, "artist-coll-bio");
            Messenger.Default.Send(false, "artist-coll-sim");

            SetArtist(id);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var queueSong = _artist.Songs.ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private async void SetArtist(int id)
        {
            LastArtist = null;
            Artist = _service.Artists.FirstOrDefault(p => p.Id == id);
            try
            {
                LastArtist = await _lastService.GetDetailArtist(Artist.Name);
                if (LastArtist == null) return;

                if (LastArtist.Similar.Count > 0)
                {
                    Messenger.Default.Send(true, "artist-coll-sim");
                }

                if (LastArtist.Bio != null && LastArtist.Bio.Content != null)
                {
                    Messenger.Default.Send(true, "artist-coll-bio");
                }
            }
            catch { }
        }
    }
}