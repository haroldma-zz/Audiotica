#region

using System;
using System.Collections.Generic;
using System.Net;
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
using Audiotica.Data.Spotify.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using MyToolkit.Multimedia;

#endregion

namespace Audiotica.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private readonly ISpotifyService _spotify;
        private IncrementalObservableCollection<SimpleAlbum> _albumsCollection;
        private Paging<SimpleAlbum> _albumsResponse;
        private IncrementalObservableCollection<FullArtist> _artistsCollection;
        private Paging<FullArtist> _artistsResponse;
        private bool _isLoading;
        private RelayCommand<KeyRoutedEventArgs> _keyDownRelayCommand;
        private string _searchTerm;
        private RelayCommand<ItemClickEventArgs> _songClickRelayCommand;
        private IncrementalObservableCollection<FullTrack> _tracksCollection;
        private Paging<FullTrack> _tracksResponse;
        private IncrementalObservableCollection<LastTrack> _lastTracksCollection;
        private PageResponse<LastTrack> _lastTrackResponse;

        public SearchViewModel(IScrobblerService service, ISpotifyService spotify)
        {
            _service = service;
            _spotify = spotify;
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

        public IncrementalObservableCollection<FullTrack> Tracks
        {
            get { return _tracksCollection; }
            set { Set(ref _tracksCollection, value); }
        }

        public IncrementalObservableCollection<LastTrack> LastTracks
        {
            get { return _lastTracksCollection; }
            set { Set(ref _lastTracksCollection, value); }
        }

        public IncrementalObservableCollection<FullArtist> Artists
        {
            get { return _artistsCollection; }
            set { Set(ref _artistsCollection, value); }
        }

        public IncrementalObservableCollection<SimpleAlbum> Albums
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
            var track = item.ClickedItem;

            if (track is LastTrack)
                await CollectionHelper.SaveTrackAsync(track as LastTrack);
            else
                await CollectionHelper.SaveTrackAsync(track as FullTrack);
        }

        public async Task SearchAsync(string term)
        {
            try
            {
                if (Tracks != null)
                {
                    Tracks.Clear();
                    _tracksResponse = null;
                }
                if (Artists != null)
                {
                    Artists.Clear();
                    _artistsResponse = null;
                }
                if (Albums != null)
                {
                    Albums.Clear();
                    _albumsResponse = null;
                }

                if (LastTracks != null)
                {
                    LastTracks.Clear();
                    _lastTrackResponse = null;
                }

                var tasks = new List<Task>
                {
                    Task.Run(async () =>
                    {
                        _tracksResponse = await _spotify.SearchTracksAsync(term);
                        await DispatcherHelper.RunAsync(() =>
                        {
                            Tracks = CreateIncrementalCollection(
                                () => _tracksResponse,
                                tracks => _tracksResponse = tracks,
                                async i => await _spotify.SearchTracksAsync(term, offset: i));
                            foreach (var lastTrack in _tracksResponse.Items)
                                Tracks.Add(lastTrack);
                        });
                    }),
                    Task.Run(async () =>
                    {
                        _albumsResponse = await _spotify.SearchAlbumsAsync(term);
                        await DispatcherHelper.RunAsync(() =>
                        {
                            Albums = CreateIncrementalCollection(
                                () => _albumsResponse,
                                albums => _albumsResponse = albums,
                                async i => await _spotify.SearchAlbumsAsync(term, offset: i));
                            foreach (var lastAlbum in _albumsResponse.Items)
                                Albums.Add(lastAlbum);
                        });
                    }),
                    Task.Run(async () =>
                    {
                        _artistsResponse = await _spotify.SearchArtistsAsync(term);
                        DispatcherHelper.RunAsync(() =>
                        {
                            Artists = CreateIncrementalCollection(
                                () => _artistsResponse,
                                artists => _artistsResponse = artists,
                                async i => await _spotify.SearchArtistsAsync(term, offset: i));
                            foreach (var lastArtist in _artistsResponse.Items)
                                Artists.Add(lastArtist);
                        });
                    }),
                     Task.Run(async () =>
                    {
                        _lastTrackResponse = await _service.SearchTracksAsync(term);
                        await DispatcherHelper.RunAsync(() =>
                        {
                            LastTracks = CreateLastIncrementalCollection(
                                () => _lastTrackResponse,
                                artists => _lastTrackResponse = artists,
                                async i => await _service.SearchTracksAsync(term, i));
                            foreach (var lastTrack in _lastTrackResponse.Content)
                                LastTracks.Add(lastTrack);
                        });
                    })
                };

                await Task.WhenAll(tasks);
            }
            catch
            {
                CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
            }
        }

        private async void KeyDownExecute(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;

            _tracksResponse = null;

            //Close the keyboard
            ((Page) ((Grid) ((TextBox) e.OriginalSource).Parent).Parent).Focus(
                FocusState.Keyboard);
            
            var term = ((TextBox) e.OriginalSource).Text;

            term = term.Trim();
            if (term.StartsWith("http://www.last.fm/music/") && term.Contains("/_/"))
            {
                CurtainPrompt.Show("Last.fm link detected.");
                term = term.Replace("http://www.last.fm/music/", "");
                var artist = term.Substring(0, term.IndexOf("/_/"));
                var title = WebUtility.UrlDecode(term.Replace(artist + "/_/", ""));
                artist = WebUtility.UrlDecode(artist);
                try
                {
                    var track = await _service.GetDetailTrack(title, artist);
                    await CollectionHelper.SaveTrackAsync(track);
                }
                catch
                {
                    CurtainPrompt.ShowError("AppNetworkIssue".FromLanguageResource());
                }
            }
            else
            {
                ((TextBox)e.OriginalSource).IsEnabled = false;
                IsLoading = true;
                await SearchAsync(term);
                ((TextBox)e.OriginalSource).IsEnabled = true;
                IsLoading = false;
            }
        }

        private IncrementalObservableCollection<T> CreateIncrementalCollection<T>(
            Func<Paging<T>> getPageResponse, Action<Paging<T>> setPageResponse,
            Func<int, Task<Paging<T>>> searchFunc) where T : new()
        {
            var collection = new IncrementalObservableCollection<T>
            {
                HasMoreItemsFunc = () =>
                {
                    var page = getPageResponse();
                    if (page != null)
                    {
                        return !string.IsNullOrEmpty(page.Next);
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

                        var pageResp = getPageResponse();
                        var resp = await searchFunc(pageResp.Offset + pageResp.Limit);

                        foreach (var item in resp.Items)
                            collection.Add(item);

                        IsLoading = false;

                        setPageResponse(resp);

                        return new LoadMoreItemsResult
                        {
                            Count = (uint) resp.Items.Count
                        };
                    }
                    catch
                    {
                        IsLoading = false;
                        setPageResponse(null);
                        CurtainPrompt.ShowError("GenericLoadingMoreError".FromLanguageResource());
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

        private IncrementalObservableCollection<T> CreateLastIncrementalCollection<T>(
            Func<PageResponse<T>> getPageResponse, Action<PageResponse<T>> setPageResponse,
            Func<int, Task<PageResponse<T>>> searchFunc) where T : new()
        {
            var collection = new IncrementalObservableCollection<T>
            {
                HasMoreItemsFunc = () =>
                {
                    var page = getPageResponse();
                    if (page != null)
                    {
                        return page.Page < page.TotalPages;
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

                        var pageResp = getPageResponse();
                        var resp = await searchFunc(pageResp.Page + 1);

                        foreach (var item in resp.Content)
                            collection.Add(item);

                        IsLoading = false;

                        setPageResponse(resp);

                        return new LoadMoreItemsResult
                        {
                            Count = (uint)resp.Content.Count
                        };
                    }
                    catch
                    {
                        IsLoading = false;
                        setPageResponse(null);
                        CurtainPrompt.ShowError("GenericLoadingMoreError".FromLanguageResource());
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