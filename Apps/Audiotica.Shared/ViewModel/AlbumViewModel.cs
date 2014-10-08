﻿#region

using System;
using System.Collections.ObjectModel;
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
            Album = await _service.GetDetailAlbum(album.Name, album.ArtistName);
            Tracks = new ObservableCollection<LastTrack>(Album.Tracks);
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