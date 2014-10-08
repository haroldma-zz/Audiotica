#region

using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Data;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    //TODO [Harry,20140908] load more items by using the load more interface in Winrt
    public class SearchViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private bool _isLoading;
        private RelayCommand<KeyRoutedEventArgs> _keyDownRelayCommand;
        private PageResponse<LastTrack> _resultsResponse;
        private string _searchTerm;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;

        public SearchViewModel(IScrobblerService service)
        {
            _service = service;
            KeyDownRelayCommand = new RelayCommand<KeyRoutedEventArgs>(KeyDownExecute);
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);

            if (IsInDesignMode)
                SearchAsync("test");
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public RelayCommand<KeyRoutedEventArgs> KeyDownRelayCommand
        {
            get { return _keyDownRelayCommand; }
            set { Set(ref _keyDownRelayCommand, value); }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set { Set(ref _searchTerm, value); }
        }

        public PageResponse<LastTrack> ResultsResponse
        {
            get { return _resultsResponse; }
            set { Set(ref _resultsResponse, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
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

        public async Task SearchAsync(string term)
        {
            try
            {
                ResultsResponse = await _service.SearchTracksAsync(term);
            }
            catch (XboxException exception)
            {
                if (!exception.Description.Contains("not exist"))
                    CurtainPrompt.ShowError("damn it! network issue...");

                //TODO [Harry,20140908] improve error notifier
                CurtainPrompt.ShowError("No search results");
            }
            catch
            {
                CurtainPrompt.ShowError("There was a network issue");
            }
        }

        private async void KeyDownExecute(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            IsLoading = true;
            ResultsResponse = null;
            ((TextBox) e.OriginalSource).IsEnabled = false;

            //Close the keyboard
            //TODO [Harry,20140908] use some linq2visual for nicer and foolproof
            ((Page) ((Panel) ((MaterialCard) ((TextBox) e.OriginalSource).Parent).Parent).Parent).Focus(
                FocusState.Keyboard);

            await SearchAsync(((TextBox) e.OriginalSource).Text);

            ((TextBox) e.OriginalSource).IsEnabled = true;
            IsLoading = false;
        }
    }
}