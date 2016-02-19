using Audiotica.Windows.Engine;

namespace Audiotica.Windows.ViewModels
{
    internal class ViewModelLocator
    {
        public static AppKernel Kernel => App.Current?.Kernel ?? AppKernelFactory.Create();

        public AboutPageViewModel AboutPage => Kernel.Resolve<AboutPageViewModel>();

        public AlbumPageViewModel AlbumPage => Kernel.Resolve<AlbumPageViewModel>();

        public AlbumsPageViewModel AlbumsPage => Kernel.Resolve<AlbumsPageViewModel>();

        public ArtistPageViewModel ArtistPage => Kernel.Resolve<ArtistPageViewModel>();

        public ArtistsPageViewModel ArtistsPage => Kernel.Resolve<ArtistsPageViewModel>();

        public ExplorePageViewModel ExplorePage => Kernel.Resolve<ExplorePageViewModel>();

        public ManualMatchPageViewModel ManualMatchPage => Kernel.Resolve<ManualMatchPageViewModel>();

        public NowPlayingPageViewModel NowPlaying => Kernel.Resolve<NowPlayingPageViewModel>();

        public SearchPageViewModel SearchPage => Kernel.Resolve<SearchPageViewModel>();

        public SettingsPageViewModel SettingsPage => Kernel.Resolve<SettingsPageViewModel>();

        public SongsPageViewModel SongsPage => Kernel.Resolve<SongsPageViewModel>();
    }
}