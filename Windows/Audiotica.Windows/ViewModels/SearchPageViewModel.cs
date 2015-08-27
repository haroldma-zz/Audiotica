using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IConverter<WebAlbum, Album> _webToAlbumConverter;
        private readonly IConverter<WebSong, Track> _webToTrackConverter;
        private List<WebAlbum> _albumsResults;
        private List<WebArtist> _artistsResults;
        private List<ISearchMetadataProvider> _searchProviders;
        private int _selectedSearchProvider;
        private List<Track> _tracksResults;

        public SearchPageViewModel(IEnumerable<IMetadataProvider> metadataProviders,
            IConverter<WebSong, Track> webToTrackConverter, IConverter<WebAlbum, Album> webToAlbumConverter,
            INavigationService navigationService)
        {
            _webToTrackConverter = webToTrackConverter;
            _webToAlbumConverter = webToAlbumConverter;
            _navigationService = navigationService;
            SearchProviders = metadataProviders.FilterAndSort<ISearchMetadataProvider>();
            SelectedSearchProvider = 0;

            SearchCommand = new Command<string>(SearchExecute);
            WebAlbumClickCommand = new Command<ItemClickEventArgs>(WebAlbumClickExecute);
            WebArtistClickCommand = new Command<ItemClickEventArgs>(WebArtistClickExecute);

            if (IsInDesignMode)
                SearchExecute("childish gambino");
        }

        public Command<ItemClickEventArgs> WebArtistClickCommand { get; }

        public Command<ItemClickEventArgs> WebAlbumClickCommand { get; }

        public Command<string> SearchCommand { get; }

        public int SelectedSearchProvider
        {
            get { return _selectedSearchProvider; }
            set { Set(ref _selectedSearchProvider, value); }
        }

        public List<ISearchMetadataProvider> SearchProviders
        {
            get { return _searchProviders; }
            set { Set(ref _searchProviders, value); }
        }

        public List<WebAlbum> AlbumsResults
        {
            get { return _albumsResults; }
            set { Set(ref _albumsResults, value); }
        }

        public List<Track> TracksResults
        {
            get { return _tracksResults; }
            set { Set(ref _tracksResults, value); }
        }

        public List<WebArtist> ArtistsResults
        {
            get { return _artistsResults; }
            set { Set(ref _artistsResults, value); }
        }

        private void WebArtistClickExecute(ItemClickEventArgs e)
        {
            var artist = (WebArtist) e.ClickedItem;
            _navigationService.Navigate(typeof (ArtistPage), artist.Name);
        }

        private void WebAlbumClickExecute(ItemClickEventArgs e)
        {
            var album = (WebAlbum) e.ClickedItem;
            _navigationService.Navigate(typeof (AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name, album) {IsCatalogMode = true});
        }

        private void SearchExecute(string query)
        {
            var provider = _searchProviders[SelectedSearchProvider];
            SearchArtists(query, provider);
            SearchAlbums(query, provider);
            SearchTracks(query, provider);
        }

        private async void SearchArtists(string query, ISearchMetadataProvider provider)
        {
            try
            {
                ArtistsResults = null;
                var result = await provider.SearchAsync(query, WebResults.Type.Artist);
                ArtistsResults = result?.Artists;
            }
            catch
            {
                CurtainPrompt.ShowError("Something happened while searching for artists!");
            }
        }

        private async void SearchAlbums(string query, ISearchMetadataProvider provider)
        {
            try
            {
                AlbumsResults = null;
                var result = await provider.SearchAsync(query, WebResults.Type.Album);
                AlbumsResults = await _webToAlbumConverter.FillPartialAsync(result?.Albums);
            }
            catch
            {
                CurtainPrompt.ShowError("Something happened while searching for albums!");
            }
        }

        private async void SearchTracks(string query, ISearchMetadataProvider provider)
        {
            try
            {
                TracksResults = null;
                var result = await provider.SearchAsync(query);
                TracksResults = await _webToTrackConverter.ConvertAsync(result?.Songs);
            }
            catch
            {
                CurtainPrompt.ShowError("Something happened while searching for tracks!");
            }
        }
    }
}