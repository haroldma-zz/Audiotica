using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Engine.Navigation;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.ViewModels
{
    public class ManualMatchPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly INavigationService _navigationService;
        private readonly IDownloadService _downloadService;
        private readonly IPlayerService _playerService;
        private readonly IEnumerable<IMatchProvider> _providers;
        private List<MatchProviderPivotItem> _providerPivots;
        private Track _track;

        public ManualMatchPageViewModel(
            IEnumerable<IMatchProvider> providers,
            ILibraryService libraryService,
            INavigationService navigationService,
            IDownloadService downloadService,
            IPlayerService playerService)
        {
            _libraryService = libraryService;
            _navigationService = navigationService;
            _downloadService = downloadService;
            _playerService = playerService;
            _providers = providers.Where(p => p.IsEnabled).OrderByDescending(p => p.Priority).ToList();
            MatchClickCommand = new DelegateCommand<MatchSong>(MatchClickExecute);

            if (IsInDesignMode)
            {
                OnNavigatedTo(0, NavigationMode.New, null);
            }
        }

        public DelegateCommand<MatchSong> MatchClickCommand { get; }

        public List<MatchProviderPivotItem> ProviderPivots
        {
            get
            {
                return _providerPivots;
            }
            set
            {
                Set(ref _providerPivots, value);
            }
        }

        public Track Track
        {
            get
            {
                return _track;
            }
            set
            {
                Set(ref _track, value);
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var id = (long)parameter;
            Track = _libraryService.Tracks.FirstOrDefault(p => p.Id == id);

            ProviderPivots = _providers.Select(p => new MatchProviderPivotItem
            {
                Title = p.DisplayName,
                Results = new ManualMatchResults(p, Track.Title, Track.DisplayArtist)
            }).ToList();
        }

        private async void MatchClickExecute(MatchSong match)
        {
            Track.AudioWebUri = match.AudioUrl;
            Track.AudioLocalUri = null;
            Track.Status = TrackStatus.None;
            Track.Type = TrackType.Stream;

            await _libraryService.UpdateTrackAsync(Track);
            await _downloadService.StartDownloadAsync(Track);
            _playerService.UpdateUrl(Track);

            _navigationService.GoBack();
        }
    }

    public class MatchProviderPivotItem
    {
        public ManualMatchResults Results { get; set; }

        public string Title { get; set; }
    }

    public class ManualMatchResults : IncrementalLoadingBase<MatchSong>
    {
        private readonly string _artist;
        private readonly IMatchProvider _provider;
        private readonly string _title;
        private bool _hasMore = true;

        public ManualMatchResults(IMatchProvider provider, string title, string artist)
        {
            _provider = provider;
            _title = title;
            _artist = artist;
        }

        protected override bool HasMoreItemsOverride() => _hasMore;

        protected override async Task<IList<MatchSong>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            try
            {
                _hasMore = false;
                var results = await _provider.GetSongsAsync(_title, _artist, verifyMatchesOnly: false);
                return results;
            }
            catch
            {
                _hasMore = true;
                return null;
            }
        }
    }
}