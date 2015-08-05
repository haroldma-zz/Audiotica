using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Factory;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class AlbumsPageViewModel : ViewModelBase
    {
        private readonly IBackgroundAudioService _audioService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly IConverter<Track, WebSong> _trackConverter;
        private readonly IMetadataProvider _metadataProvider;

        public AlbumsPageViewModel(IBackgroundAudioService audioService, IMetadataProvider metadataProvider,
            IMatchEngineService matchEngineService, IConverter<Track, WebSong> trackConverter)
        {
            _audioService = audioService;
            _metadataProvider = metadataProvider;
            _matchEngineService = matchEngineService;
            _trackConverter = trackConverter;
        }

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var webResults = await _metadataProvider.SearchAsync("the weeknd earned it");
            var song = webResults.Songs[1];
            var track = await _trackConverter.ConvertAsync(song);
            track.AudioWebUri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
            _audioService.SwitchTo(new List<Track> {track});
        }
    }
}