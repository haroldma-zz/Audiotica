using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public sealed class ArtistPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IMetadataProvider> _metadataProviders;
        private readonly INavigationService _navigationService;
        private readonly IConverter<WebArtist, Artist> _webArtistConverter;
        private readonly IConverter<WebSong, Track> _webSongConverter;
        private Artist _artist;
        private List<WebAlbum> _topAlbums;
        private List<Track> _topSongs;

        public ArtistPageViewModel(INavigationService navigationService,
            ILibraryService libraryService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IConverter<WebArtist, Artist> webArtistConverter,
            IConverter<WebSong, Track> webSongConverter)
        {
            _navigationService = navigationService;
            _libraryService = libraryService;
            _metadataProviders = metadataProviders.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority).ToList();
            _webArtistConverter = webArtistConverter;
            _webSongConverter = webSongConverter;

            if (IsInDesignMode)
                OnNavigatedTo(1, NavigationMode.New, new Dictionary<string, object>());
        }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<WebAlbum> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public List<Track> TopSongs
        {
            get { return _topSongs; }
            set { Set(ref _topSongs, value); }
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var id = parameter as long?;

            if (id != null)
            {
                // TODO get the artist from the library
            }
            else
            {
                var detokenized = parameter.ToString().DeTokenize();
                var provider = Type.GetType(detokenized[0]);
                var webToken = detokenized[1];
                var name = detokenized[2];

                var metadataProvider = _metadataProviders.FirstOrDefault(p => p.GetType() == provider);
                try
                {
                    // TODO: try to find the artist in the library

                    // otherwise get it from the provider
                    var webArtist = await metadataProvider.GetArtistAsync(webToken);
                    if (webArtist == null)
                    {
                        _navigationService.GoBack();
                        return;
                    }
                    Artist = await _webArtistConverter.ConvertAsync(webArtist);
                }
                catch (Exception e)
                {
                    if (e is ProviderException)
                        CurtainPrompt.ShowError(e.Message);
                    _navigationService.GoBack();
                    return;
                }
            }
            LoadWebData();
        }

        private async void LoadWebData()
        {
            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var webArtist = await metadataProvider.GetArtistByNameAsync(Artist.Name);
                    if (webArtist == null) continue;

                    if (TopSongs == null)
                        TopSongs = await _webSongConverter.ConvertAsync(
                            (await metadataProvider.GetArtistTopSongsAsync(webArtist.Token, 10)).Songs);

                    if (TopAlbums == null)
                        TopAlbums = (await metadataProvider.GetArtistAlbumsAsync(webArtist.Token, 10)).Albums;

                    if (TopSongs != null && TopAlbums != null) break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}