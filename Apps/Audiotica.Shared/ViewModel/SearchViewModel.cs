#region

using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private IncrementalObservableCollection<LastAlbum> _albumsCollection;
        private PageResponse<LastAlbum> _albumsResponse;
        private IncrementalObservableCollection<LastArtist> _artistsCollection;
        private PageResponse<LastArtist> _artistsResponse;
        private bool _isLoading;
        private RelayCommand<KeyRoutedEventArgs> _keyDownRelayCommand;
        private string _searchTerm;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private IncrementalObservableCollection<LastTrack> _tracksCollection;
        private PageResponse<LastTrack> _tracksResponse;

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

        public IncrementalObservableCollection<LastTrack> Tracks
        {
            get { return _tracksCollection; }
            set { Set(ref _tracksCollection, value); }
        }

        public IncrementalObservableCollection<LastArtist> Artists
        {
            get { return _artistsCollection; }
            set { Set(ref _artistsCollection, value); }
        }

        public IncrementalObservableCollection<LastAlbum> Albums
        {
            get { return _albumsCollection; }
            set { Set(ref _albumsCollection, value); }
        }

        public RelayCommand<ItemClickEventArgs> SongClickRelayCommand
        {
            get { return _songClickRelayCommand; }
            set { Set(ref _songClickRelayCommand, value); }
        }

        private async void SongClickExecute(ItemClickEventArgs item)
        {
            var track = (LastTrack) item.ClickedItem;

            CurtainToast.Show("MatchingSongToast".FromLanguageResource());
            await ScrobblerHelper.SaveTrackAsync(track);
        }

        public async Task SearchAsync(string term)
        {
            try
            {
                if (Tracks != null)
                {
                    Tracks.Clear();
                }
                if (Artists != null)
                {
                    Artists.Clear();
                }
                if (Albums != null)
                {
                    Albums.Clear();
                }

                _tracksResponse = await _service.SearchTracksAsync(term);

                Tracks = CreateIncrementalCollection(
                    () => _tracksResponse,
                    tracks => _tracksResponse = tracks,
                    async i => await _service.SearchTracksAsync(term, i));
                foreach (var lastTrack in _tracksResponse)
                    Tracks.Add(lastTrack);

                _albumsResponse = await _service.SearchAlbumsAsync(term);

                Albums = CreateIncrementalCollection(
                    () => _albumsResponse,
                    albums => _albumsResponse = albums,
                    async i => await _service.SearchAlbumsAsync(term, i));
                foreach (var lastAlbum in _albumsResponse)
                    Albums.Add(lastAlbum);

                _artistsResponse = await _service.SearchArtistAsync(term);

                Artists = CreateIncrementalCollection(
                    () => _artistsResponse,
                    artists => _artistsResponse = artists,
                    async i => await _service.SearchArtistAsync(term, i));
                foreach (var lastArtist in _artistsResponse)
                    Artists.Add(lastArtist);

                //if (_tracksResponse.TotalItems == 0)
                //CurtainToast.ShowError("NoSearchResultsToast".FromLanguageResource());
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
            _tracksResponse = null;
            ((TextBox) e.OriginalSource).IsEnabled = false;

            //Close the keyboard
            ((Page) ((Grid) ((TextBox) e.OriginalSource).Parent).Parent).Focus(
                FocusState.Keyboard);

            await SearchAsync(((TextBox) e.OriginalSource).Text);

            ((TextBox) e.OriginalSource).IsEnabled = true;
            IsLoading = false;
        }

        private IncrementalObservableCollection<T> CreateIncrementalCollection<T>(
            Func<PageResponse<T>> getPageResponse, Action<PageResponse<T>> setPageResponse,
            Func<int, Task<PageResponse<T>>> searchFunc) where T : new()
        {
            var collection = new IncrementalObservableCollection<T>
            {
                HasMoreItemsFunc = () =>
                {
                    if (getPageResponse() != null)
                    {
                        return getPageResponse().Page < getPageResponse().TotalPages;
                    }
                    return false;
                }
            };

            collection.LoadMoreItemsFunc = count =>
            {
                Func<Task<LoadMoreItemsResult>> taskFunc = async () =>
                {
                    try
                    {
                        IsLoading = true;

                        var resp = await searchFunc(getPageResponse().Page + 1);

                        foreach (var item in resp.Content)
                            collection.Add(item);

                        IsLoading = false;

                        setPageResponse(resp);

                        return new LoadMoreItemsResult
                        {
                            Count = (uint) resp.Content.Count
                        };
                    }
                    catch
                    {
                        IsLoading = false;
                        setPageResponse(null);
                        CurtainToast.ShowError("Problem loading more items.");
                        return new LoadMoreItemsResult
                        {
                            Count = 0
                        };
                    }
                };
                var loadMorePostsTask = taskFunc();
                return loadMorePostsTask.AsAsyncOperation();
            };
            return collection;
        }
    }
}