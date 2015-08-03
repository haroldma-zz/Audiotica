using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class MainPageViewModel : ViewModelBase
    {
        private readonly IMatchEngineService _matchEngineService;
        private readonly IBackgroundAudioService _backgroundAudioService;
        private readonly IMetadataProvider[] _metadataProviders;

        public MainPageViewModel(IMetadataProvider[] metadataProviders, IMatchEngineService matchEngineService, IBackgroundAudioService backgroundAudioService)
        {
            _metadataProviders = metadataProviders;
            _matchEngineService = matchEngineService;
            _backgroundAudioService = backgroundAudioService;
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var metadataProvider = _metadataProviders.FirstOrDefault(p => p is SpotifyMetadataProvider);

            var tracks = new List<Track>();
            var results = await metadataProvider.SearchAsync("justin bieber");
            int i = 0;
            foreach (var track in results.Songs.Select(webSong => new Track
            {
                Title = webSong.Title,
                Artists = string.Join(";", webSong.Artists.Select(p => p.Name)),
                Album = webSong.Album.Name,
                AlbumArtist = webSong.Album.Artist?.Name ?? webSong.Artists.Select(p => p.Name).FirstOrDefault(),
                ArtworkUri = webSong.Album.Artwork
            }))
            {
                track.AudioUri = (await _matchEngineService.GetLinkAsync(track.Title, track.AlbumArtist));
                Debug.WriteLine(i++);
                if (track.AudioUri != null)
                    tracks.Add(track);
            }
            
            _backgroundAudioService.Play(tracks);
        }
    }
}