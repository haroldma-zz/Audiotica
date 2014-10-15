#region

using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
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
            var track = (LastTrack)item.ClickedItem;

            CurtainToast.Show("MatchingSongToast".FromLanguageResource());
            await ScrobblerHelper.SaveTrackAsync(track);
        }

        public async Task SearchAsync(string term)
        {
            try
            {
                ResultsResponse = await _service.SearchTracksAsync(term);
                if (ResultsResponse.TotalItems == 0)
                    CurtainToast.ShowError("NoSearchResultsToast".FromLanguageResource());
            }
            catch (LastException ex)
            {
                CurtainToast.ShowError(ex.Message);
            }
            catch
            {
                CurtainToast.ShowError("NetworkIssueToast".FromLanguageResource());
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