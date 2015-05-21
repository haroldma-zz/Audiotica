using Audiotica.AppEngine;
using Audiotica.Factories;

namespace Audiotica.ViewModels
{
    internal class ViewModelLocator
    {
        private readonly AppKernel _kernel;

        public ViewModelLocator()
        {
            _kernel = App.Current?.AppKernel ?? AppKernelFactory.Create();
        }

        public MainViewModel Main => _kernel.Resolve<MainViewModel>();
        public SongsViewModel Songs => _kernel.Resolve<SongsViewModel>();
        public ArtistsViewModel Artists => _kernel.Resolve<ArtistsViewModel>();
        public AlbumsViewModel Albums => _kernel.Resolve<AlbumsViewModel>();
        public ArtistViewModel Artist => _kernel.Resolve<ArtistViewModel>();
        public AlbumViewModel Album => _kernel.Resolve<AlbumViewModel>();
    }
}