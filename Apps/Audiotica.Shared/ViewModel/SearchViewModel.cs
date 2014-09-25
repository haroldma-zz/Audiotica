#region

using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Data;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.ViewModel
{
    //TODO [Harry,20140908] load more items by using the load more interface in Winrt
    public class SearchViewModel : ViewModelBase
    {
        private readonly IXboxMusicService _service;
        private bool _isLoading;
        private RelayCommand<KeyRoutedEventArgs> _keyDownRelayCommand;
        private ContentResponse _resultsResponse;
        private string _searchTerm;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;

        public SearchViewModel(IXboxMusicService service)
        {
            _service = service;
            KeyDownRelayCommand = new RelayCommand<KeyRoutedEventArgs>(KeyDownExecute);
            SongClickRelayCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);

            if (IsInDesignMode)
                SearchAsync("test");
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

        public ContentResponse ResultsResponse
        {
            get { return _resultsResponse; }
            set { Set(ref _resultsResponse, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        public async Task SearchAsync(string term)
        {
            try
            {
                ResultsResponse = await _service.Search(term);
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

            ((TextBox)e.OriginalSource).IsEnabled = true;
            IsLoading = false;
        }
    }
}