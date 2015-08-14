using Audiotica.Windows.AppEngine;
using Audiotica.Windows.Factories;

namespace Audiotica.Windows.ViewModels
{
    internal class ViewModelLocator
    {
        public static AppKernel Kernel => App.Current?.Kernel ?? AppKernelFactory.Create();
        public ExplorePageViewModel ExplorePage => Kernel.Resolve<ExplorePageViewModel>();
        public AlbumsPageViewModel AlbumsPage => Kernel.Resolve<AlbumsPageViewModel>();
        public AlbumPageViewModel AlbumPage => Kernel.Resolve<AlbumPageViewModel>();
        public ArtistsPageViewModel ArtistsPage => Kernel.Resolve<ArtistsPageViewModel>();
        public ArtistPageViewModel ArtistPage => Kernel.Resolve<ArtistPageViewModel>();
        public SongsPageViewModel SongsPage => Kernel.Resolve<SongsPageViewModel>();
        public NowPlayingPageViewModel NowPlaying => Kernel.Resolve<NowPlayingPageViewModel>();
        public AboutPageViewModel AboutPage => Kernel.Resolve<AboutPageViewModel>();
        public SettingsPageViewModel SettingsPage => Kernel.Resolve<SettingsPageViewModel>();
    }
}