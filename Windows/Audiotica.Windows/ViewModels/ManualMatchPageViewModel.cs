using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class ManualMatchPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly IEnumerable<IMatchProvider> _providers;
        private List<MatchProviderPivotItem> _providerPivots;
        private Track _track;

        public ManualMatchPageViewModel(IEnumerable<IMatchProvider> providers, ILibraryService libraryService)
        {
            _libraryService = libraryService;
            _providers = providers.Where(p => p.IsEnabled).OrderByDescending(p => p.Priority).ToList();
            MatchClickCommand = new Command<ItemClickEventArgs>(MatchClickExecute);

            if (IsInDesignMode)
                OnNavigatedTo(0, NavigationMode.New, null);
        }

        public Track Track
        {
            get { return _track; }
            set { Set(ref _track, value); }
        }

        public Command<ItemClickEventArgs> MatchClickCommand { get; }

        public List<MatchProviderPivotItem> ProviderPivots
        {
            get { return _providerPivots; }
            set { Set(ref _providerPivots, value); }
        }

        private async void MatchClickExecute(ItemClickEventArgs e)
        {
            var match = (MatchSong) e.ClickedItem;
            // TODO: Update queue items that belong to this track
            Track.AudioWebUri = match.AudioUrl;
            await _libraryService.UpdateTrackAsync(Track);
        }

        public override sealed void OnNavigatedTo(object parameter, NavigationMode mode,
            Dictionary<string, object> state)
        {
            var id = (long) parameter;
            Track = _libraryService.Tracks.FirstOrDefault(p => p.Id == id);

            ProviderPivots = _providers.Select(p => new MatchProviderPivotItem
            {
                Title = p.DisplayName,
                Results = new ManualMatchResults(p, Track.Title, Track.DisplayArtist)
            }).ToList();
        }
    }

    public class MatchProviderPivotItem
    {
        public string Title { get; set; }
        public ManualMatchResults Results { get; set; }
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

        protected override async Task<IList<MatchSong>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            var results = await _provider.GetSongsAsync(_title, _artist, verifyMatchesOnly: false);
            _hasMore = false;
            return results;
        }

        protected override bool HasMoreItemsOverride() => _hasMore;
    }
}