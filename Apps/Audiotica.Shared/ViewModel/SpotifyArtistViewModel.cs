#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data;
using Audiotica.Data.Model.Spotify.Models;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class SpotifyArtistViewModel : ViewModelBase
    {
        private readonly ISpotifyService _service;
        private FullArtist _artist;
        private bool _isLoading;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private Paging<SimpleAlbum> _topAlbums;
        private List<FullTrack> _topTracks;

        public SpotifyArtistViewModel(ISpotifyService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "spotify-artist-detail-id", ReceivedName);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public FullArtist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<FullTrack> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public Paging<SimpleAlbum> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private void ReceivedName(GenericMessage<string> msg)
        {
            if (Artist != null && msg.Content == Artist.Name) return;

            Artist = null;
            TopAlbums = null;
            TopTracks = null;
            LoadData(msg.Content);
        }

        public async void LoadData(string id)
        {
            IsLoading = true;

            try
            {
                Artist = await _service.GetArtistAsync(id);
            }
            catch (Exception e)
            {
                CurtainToast.ShowError(e.Message);
            }
            try
            {
                TopTracks = await _service.GetArtistTracksAsync(id);
            }
            catch (Exception e)
            {
                CurtainToast.ShowError(e.Message);
            }

            try
            {
                TopAlbums = await _service.GetArtistAlbumsAsync(id);
            }
            catch (Exception e)
            {
                CurtainToast.ShowError(e.Message);
            }

            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (FullTrack)item.ClickedItem;
            var album = await _service.GetAlbumAsync(track.Album.Id);

            CurtainToast.Show("MatchingSongToast".FromLanguageResource());
            await CollectionHelper.SaveTrackAsync(track, album);
        }
    }
}