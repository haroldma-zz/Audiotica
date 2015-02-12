#region

using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

using Audiotica.Core.Exceptions;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Spotify;
using Audiotica.Data.Spotify.Models;
using Audiotica.View;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.ViewModel
{
    public class SpotifyArtistViewModel : ViewModelBase
    {
        private readonly ISpotifyService _service;
        private readonly INotificationManager _notificationManager;
        private SimpleArtist _artist;
        private bool _isLoading;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private Paging<SimpleAlbum> _topAlbums;
        private List<FullTrack> _topTracks;

        public SpotifyArtistViewModel(ISpotifyService service, INotificationManager notificationManager)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;
            _notificationManager = notificationManager;

            MessengerInstance.Register<GenericMessage<string>>(this, "spotify-artist-detail-id", ReceivedName);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public SimpleArtist Artist
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
                if (id.StartsWith("name."))
                {
                    var name = id.Replace("name.", string.Empty);
                    var spotify = await App.Locator.Spotify.SearchItems(name, SearchType.ARTIST, 1);

                    if (spotify != null && spotify.Artists != null && spotify.Artists.Items.Count > 0)
                    {
                        id = spotify.Artists.Items[0].Id;
                    }
                    else
                    {
                        // not found on spotify, go to lastfm
                        App.Navigator.GoTo<ArtistPage, PageTransition>(name, false);
                        return;
                    }
                }
                Artist = await _service.GetArtistAsync(id);
            }
            catch (NetworkException e)
            {
                _notificationManager.ShowError("AppNetworkIssue".FromLanguageResource());
            }
            try
            {
                TopTracks = await _service.GetArtistTracksAsync(id);
            }
            catch (Exception e)
            {
                _notificationManager.ShowError("AppNetworkIssue".FromLanguageResource());
            }

            try
            {
                TopAlbums = await _service.GetArtistAlbumsAsync(id);
            }
            catch (Exception e)
            {
                _notificationManager.ShowError("AppNetworkIssue".FromLanguageResource());
            }

            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (FullTrack) item.ClickedItem;
            await CollectionHelper.SaveTrackAsync(track);
        }
    }
}