#region

using System.Collections.Generic;
using System.Linq;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.ViewModel
{
    public class ArtistViewModel : ViewModelBase
    {
        private readonly IXboxMusicService _service;
        private XboxArtist _artist;
        private bool _isLoading;
        private List<XboxAlbum> _topAlbums;
        private List<XboxTrack> _topTracks;

        public ArtistViewModel(IXboxMusicService service)
        {
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "artist-detail-id", ReceivedId);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public XboxArtist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<XboxTrack> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public List<XboxAlbum> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private void ReceivedId(GenericMessage<string> msg)
        {
            Artist = null;
            TopAlbums = null;
            TopTracks = null;
            LoadData(msg.Content);
        }

        public async void LoadData(string id)
        {
            IsLoading = true;
            Artist = await _service.GetArtistDetails(id);
            TopTracks = Artist.TopTracks.Items.Take(5).ToList();
            TopAlbums = Artist.Albums.Items.Take(5).ToList();
            IsLoading = false;
        }
    }
}