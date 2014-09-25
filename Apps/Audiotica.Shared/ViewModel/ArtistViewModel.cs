#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Data;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;

        public ArtistViewModel(IXboxMusicService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "artist-detail-id", ReceivedId);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
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
            if (Artist != null && msg.Content == Artist.Id) return;

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

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var xboxTrack = item.ClickedItem as XboxTrack;

            //TODO [Harry,20140909] use a ui blocker with progress indicator
            IsLoading = true;

            var url = await Mp3MatchEngine.FindMp3For(xboxTrack);

            IsLoading = false;

            if (url == null)
            {
                CurtainPrompt.ShowError("No match found");
            }

            else
            {
                var song = xboxTrack.ToSong();
                song.AudioUrl = url;
                try
                {
                    await App.Locator.CollectionService.AddSongAsync(song, xboxTrack.ImageUrl);
                    CurtainPrompt.Show("Song saved");
                }
                catch (Exception e)
                {
                    CurtainPrompt.ShowError(e.Message);
                }
            }
        }
    }
}