#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.ViewModel
{
    public class AlbumViewModel : ViewModelBase
    {
        private readonly IXboxMusicService _service;
        private XboxAlbum _album;
        private bool _isLoading;
        private ObservableCollection<XboxTrack> _tracks;

        public AlbumViewModel(IXboxMusicService service)
        {
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "album-detail-id", ReceivedId);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public XboxAlbum Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public ObservableCollection<XboxTrack> Tracks
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
            Album = null;
            Tracks = null;
            LoadData(msg.Content);
        }

        private async void LoadData(string albumId)
        {
            IsLoading = true;
            Album = await _service.GetAlbumDetails(albumId);
            Tracks = new ObservableCollection<XboxTrack>(Album.Tracks.Items);
            IsLoading = false;
        }
    }
}