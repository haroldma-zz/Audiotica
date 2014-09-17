#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Data;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
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
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;

        public AlbumViewModel(IXboxMusicService service)
        {
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
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

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
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

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var xboxTrack = item.ClickedItem as XboxTrack;

            //TODO [Harry,20140909] use a ui blocker with progress indicator
            IsLoading = true;

            //TODO [Harry,20140908] actual downloading instead of previewing
            var url = await Mp3MatchEngine.FindMp3For(xboxTrack);

            IsLoading = false;

            if (url == null)
            {
                await new MessageDialog("no match found :/").ShowAsync();
            }

            else
            {
                xboxTrack.Album = Album;
                var song = xboxTrack.ToSong();
                song.AudioUrl = url;
                await App.Locator.CollectionService.AddSongAsync(song, xboxTrack.ImageUrl);

                //TODO [Harry,20140917] notification here
            }
        }
    }
}