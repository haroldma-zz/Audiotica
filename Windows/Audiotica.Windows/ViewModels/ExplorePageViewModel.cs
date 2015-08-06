using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Factory;
using Audiotica.Web.Exceptions;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal sealed class ExplorePageViewModel : ViewModelBase
    {
        private readonly IConverter<Track, WebSong> _webSongConverter;
        private readonly IBackgroundAudioService _backgroundAudioService;
        private readonly IMatchEngineService _matchEngineService;
        private List<WebAlbum> _topAlbums;
        private List<WebArtist> _topArtists;
        private List<WebSong> _topSongs;

        public ExplorePageViewModel(IEnumerable<IMetadataProvider> metadataProviders, IConverter<Track, WebSong> webSongConverter, IBackgroundAudioService backgroundAudioService, IMatchEngineService matchEngineService)
        {
            _webSongConverter = webSongConverter;
            _backgroundAudioService = backgroundAudioService;
            _matchEngineService = matchEngineService;
            MetadataProviders = metadataProviders.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority).ToList();

            SongClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);

            if (IsInDesignMode)
                OnNavigatedTo(null, NavigationMode.New, new Dictionary<string, object>());
        }

        public Command<ItemClickEventArgs> SongClickCommand { get; }
        public List<IMetadataProvider> MetadataProviders { get; }

        public List<WebSong> TopSongs
        {
            get { return _topSongs; }
            set { Set(ref _topSongs, value); }
        }

        public List<WebAlbum> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public List<WebArtist> TopArtists
        {
            get { return _topArtists; }
            set { Set(ref _topArtists, value); }
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as WebSong;
            CurtainPrompt.Show("Getting data...");
            var track = await _webSongConverter.ConvertAsync(song);
            CurtainPrompt.Show("Matching...");
            track.AudioWebUri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
            _backgroundAudioService.Play(new List<Track> {track});
        }

        public override void OnNavigatedTo(string parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            LoadTopSongs();
            LoadTopArtists();
            LoadTopAlbums();
        }

        private async void LoadTopSongs()
        {
            foreach (var metadataProvider in MetadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopSongsAsync(20);
                    TopSongs = results.Songs;
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }

        private async void LoadTopArtists()
        {
            foreach (var metadataProvider in MetadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopArtistsAsync(20);
                    TopArtists = results.Artists;
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }

        private async void LoadTopAlbums()
        {
            foreach (var metadataProvider in MetadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopAlbumsAsync(20);
                    TopAlbums = results.Albums;
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }
    }
}