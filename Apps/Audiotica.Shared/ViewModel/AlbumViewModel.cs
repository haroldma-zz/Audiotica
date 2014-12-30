#region

using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class AlbumViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private LastAlbum _album;
        private bool _isLoading;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private ObservableCollection<LastTrack> _tracks;

        public AlbumViewModel(IScrobblerService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;

            MessengerInstance.Register<GenericMessage<LastAlbum>>(this, "album-detail", ReceivedId);

            if (IsInDesignMode)
                LoadData(new LastAlbum());
        }

        public LastAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public ObservableCollection<LastTrack> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private void ReceivedId(GenericMessage<LastAlbum> msg)
        {
            if (Album != null && msg.Content.Name == Album.Name) return;

            Album = null;
            Tracks = null;
            LoadData(msg.Content);
        }

        private async void LoadData(LastAlbum album)
        {
            IsLoading = true;

            try
            {
                Album = await _service.GetDetailAlbum(album.Name, album.ArtistName);

                if (Album.Tracks == null && Album.Name.Contains("Deluxe Edition"))
                {
                    Album = await _service.GetDetailAlbum(album.Name.Replace("(Deluxe Edition)", ""), album.ArtistName);
                }

                if (Album.Tracks != null)
                    Tracks = new ObservableCollection<LastTrack>(Album.Tracks);
                else
                    CurtainPrompt.ShowError("AlbumNoTracks".FromLanguageResource());
            }
            catch (Exception e)
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }
            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (LastTrack) item.ClickedItem;
            await CollectionHelper.SaveTrackAsync(track);
        }
    }
}