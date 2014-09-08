#region

using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
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
        private Visibility _resultsVisibility = Visibility.Collapsed;
        private string _searchTerm;

        public SearchViewModel(IXboxMusicService service)
        {
            _service = service;
            KeyDownRelayCommand = new RelayCommand<KeyRoutedEventArgs>(KeyDownExecute);

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

        public ContentResponse ResultsResponse
        {
            get { return _resultsResponse; }
            set { Set(ref _resultsResponse, value); }
        }

        public Visibility ResultsVisibility
        {
            get { return _resultsVisibility; }
            set { Set(ref _resultsVisibility, value); }
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
                    throw;

                //TODO [Harry,20140908] improve error notifier
                new MessageDialog("No search results").ShowAsync();
            }
        }

        private async void KeyDownExecute(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            IsLoading = true;
            ResultsResponse = null;
            ((TextBox) e.OriginalSource).IsEnabled = false;
            ResultsVisibility = Visibility.Visible;

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