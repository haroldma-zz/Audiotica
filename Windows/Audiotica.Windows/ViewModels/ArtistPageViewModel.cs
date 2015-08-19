using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Extensions;
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
        private readonly List<IExtendedMetadataProvider> _metadataProviders;
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
            _metadataProviders = metadataProviders.FilterAndSort<IExtendedMetadataProvider>();

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

        public override string SimplifiedParameter(object parameter)
        {
            var id = parameter as long?;

            return id != null ? id.ToString() : parameter.ToString().DeTokenize().LastOrDefault();
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var name = parameter as string;

            if (name != null)
            {
                Artist = _libraryService.Artists.FirstOrDefault(p => p.Name == name);
            }
            else
            {
                var webArtist = (WebArtist) parameter;
                Artist = await _webArtistConverter.ConvertAsync(webArtist);
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