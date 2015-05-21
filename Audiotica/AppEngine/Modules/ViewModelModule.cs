using Audiotica.ViewModels;
using Autofac;

namespace Audiotica.AppEngine.Modules
{
    internal class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<SongsViewModel>();
            builder.RegisterType<ArtistsViewModel>();
            builder.RegisterType<AlbumsViewModel>();
            builder.RegisterType<ArtistViewModel>();
            builder.RegisterType<AlbumViewModel>();
        }
    }
}