using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;

namespace Audiotica.ViewModels
{
    internal class MainViewModel : NavigatableViewModel
    {
        private readonly IMatchEngineService _matchEngineService;
        private readonly IMetadataProvider _metadataProvider;

        public MainViewModel(IMatchEngineService matchEngineService, IMetadataProvider metadataProvider)
        {
            _matchEngineService = matchEngineService;
            _metadataProvider = metadataProvider;
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var results = await _metadataProvider.SearchAsync("chris brown");
            var song = results.Songs[0];

            var url = await _matchEngineService.GetLinkAsync(song.Title, song.Artist.Name);
            // debug purposes
        }
    }
}