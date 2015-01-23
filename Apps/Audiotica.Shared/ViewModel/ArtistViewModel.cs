#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
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

            try
            {
                Artist = await _service.GetDetailArtist(name);
            }
            catch
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }
            try
            {
                TopTracks = (await _service.GetArtistTopTracks(name)).Content.ToList();
            }
            catch
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }

            try
            {
                TopAlbums = (await _service.GetArtistTopAlbums(name)).Content.ToList();
            }
            catch
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }

            IsLoading = false;
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (LastTrack)item.ClickedItem;
            await CollectionHelper.SaveTrackAsync(track);
        }
    }
}