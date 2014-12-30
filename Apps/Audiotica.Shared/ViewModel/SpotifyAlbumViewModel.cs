#region

using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Spotify.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class SpotifyAlbumViewModel : ViewModelBase
    {
        private FullAlbum _album;
        private bool _isLoading;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private ObservableCollection<SimpleTrack> _tracks;
        private readonly ISpotifyService _service;

        public SpotifyAlbumViewModel(ISpotifyService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "spotify-album-detail", ReceivedId);

            if (IsInDesignMode)
                LoadData("");
        }

        public FullAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public ObservableCollection<SimpleTrack> Tracks
        {
            get { return _tracks; }
            set { Set(ref _tracks, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private void ReceivedId(GenericMessage<string> msg)
        {
            if (Album != null && msg.Content == Album.Id) return;

            Album = null;
            Tracks = null;
            LoadData(msg.Content);
        }

        private async void LoadData(string id)
        {
            IsLoading = true;

            try
            {
                Album = await _service.GetAlbumAsync(id);
                foreach (var simpleTrack in Album.Tracks.Items)
                {
                    simpleTrack.Artist = Album.Artists[0];
                }
                Tracks = new ObservableCollection<SimpleTrack>(Album.Tracks.Items);
            }
            catch
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }
            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (SimpleTrack)item.ClickedItem;
            await CollectionHelper.SaveTrackAsync(track, Album);
        }
    }
}