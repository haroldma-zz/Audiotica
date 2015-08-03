using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class MetadataProviderModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<SpotifyMetadataProvider>().As<IMetadataProvider>();
            builder.RegisterType<LastFmMetadataProvider>().As<IMetadataProvider>();
        }
    }
}