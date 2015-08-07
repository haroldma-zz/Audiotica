using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal sealed class ArtistPageViewModel : ViewModelBase
    {
        private readonly List<IMetadataProvider> _metadataProviders;
        private readonly INavigationService _navigationService;
        private readonly IConverter<WebArtist, Artist> _webArtistConverter;
        private Artist _artist;
        private List<WebSong> _topSongs;
        private List<WebAlbum> _topAlbums;

        public ArtistPageViewModel(INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IConverter<WebArtist, Artist> webArtistConverter)
        {
            _navigationService = navigationService;
            _metadataProviders = metadataProviders.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority).ToList();
            _webArtistConverter = webArtistConverter;

            if (IsInDesignMode)
                OnNavigatedTo("1", NavigationMode.New, new Dictionary<string, object>());
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

        public List<WebSong> TopSongs
        {
            get { return _topSongs; }
            set { Set(ref _topSongs, value); }
        }

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            long id;

            if (long.TryParse(parameter, out id))
            {
                // TODO get the artist from the library
            }
            else
            {
                var detokenized = parameter.DeTokenize();
                var provider = Type.GetType(detokenized[0]);
                var webToken = detokenized[1];
                var name = detokenized[2];

                var metadataProvider = _metadataProviders.FirstOrDefault(p => p.GetType() == provider);
                try
                {
                    // try to find the artist in the library

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

                    await LoadWebData(metadataProvider, webArtist.Token);

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

        private async Task LoadWebData(IMetadataProvider metadataProvider, string webToken)
        {
            if (TopSongs == null)
                TopSongs = (await metadataProvider.GetArtistTopSongsAsync(webToken)).Songs;

            if (TopAlbums == null)
                TopAlbums = (await metadataProvider.GetArtistAlbumsAsync(webToken)).Albums;
        }
    }
}