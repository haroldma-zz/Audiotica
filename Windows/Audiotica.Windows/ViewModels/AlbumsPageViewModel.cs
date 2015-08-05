using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class AlbumsPageViewModel : ViewModelBase
    {
        private readonly IBackgroundAudioService _audioService;
        private readonly IMetadataProvider _metadataProvider;
        private readonly IMatchEngineService _matchEngineService;

        public AlbumsPageViewModel(IBackgroundAudioService audioService, IMetadataProvider metadataProvider, IMatchEngineService matchEngineService)
        {
            _audioService = audioService;
            _metadataProvider = metadataProvider;
            _matchEngineService = matchEngineService;
        }

        public async override void OnNavigatedTo(string parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var webResults = await _metadataProvider.SearchAsync("austin mahone");
            var song = webResults.Songs[0];
            var track = new Track
            {
                Title = song.Title,
                Artists = string.Join(";", song.Artists.Select(p => p.Name)),
                AlbumTitle = song.Album.Name,
                AlbumArtist = song.Album.Artist?.Name ?? song.Artists[0].Name,
                ArtworkUri = song.Album.Artwork,
                DisplayArtist = song.Artists[0].Name
            };
            track.AudioUri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
            _audioService.Update(new List<Track> {track });
        }
    }
}