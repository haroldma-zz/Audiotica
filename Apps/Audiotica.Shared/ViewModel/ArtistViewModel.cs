#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Data;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class ArtistViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private LastArtist _artist;
        private bool _isLoading;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private List<LastAlbum> _topAlbums;
        private List<LastTrack> _topTracks;

        public ArtistViewModel(IScrobblerService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            _service = service;

            MessengerInstance.Register<GenericMessage<string>>(this, "artist-detail-name", ReceivedName);

            if (IsInDesignMode)
                LoadData("music.test");
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public LastArtist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<LastTrack> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public List<LastAlbum> TopAlbums
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

        public async void LoadData(string name)
        {
            IsLoading = true;
            Artist = await _service.GetDetailArtist(name);
            TopTracks = (await _service.GetArtistTopTracks(name)).Content.ToList();
            TopAlbums = (await _service.GetArtistTopAlbums(name)).Content.ToList();
            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var lastTrack = item.ClickedItem as LastTrack;

            //TODO [Harry,20140909] use a ui blocker with progress indicator
            IsLoading = true;

            //TODO [Harry,20140908] actual downloading instead of previewing
            var url = await Mp3MatchEngine.FindMp3For(lastTrack);

            IsLoading = false;

            if (url == null)
            {
                CurtainPrompt.ShowError("No match found");
            }

            else
            {
                lastTrack = await _service.GetDetailTrack(lastTrack.Name, lastTrack.ArtistName);
                var song = lastTrack.ToSong();
                LastArtist lastArtist;

                if (!string.IsNullOrEmpty(lastTrack.AlbumName))
                {
                    var lastAlbum = await _service.GetDetailAlbum(lastTrack.AlbumName, lastTrack.ArtistName);
                    lastArtist = await _service.GetDetailArtistByMbid(lastTrack.ArtistMbid);
                    song.Album = lastAlbum.ToAlbum();
                    song.Album.PrimaryArtist = lastArtist.ToArtist();
                    lastTrack.Images = lastAlbum.Images;
                }

                else
                    lastArtist = await _service.GetDetailArtist(lastTrack.ArtistName);

                song.Artist = lastArtist.ToArtist();
                song.ArtistName = lastArtist.Name;

                song.AudioUrl = url;
                try
                {
                    await App.Locator.CollectionService.AddSongAsync(song, lastTrack.Images != null ? lastTrack.Images.Largest.AbsoluteUri : null);
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