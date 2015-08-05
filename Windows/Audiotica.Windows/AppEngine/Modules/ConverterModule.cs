using Audiotica.Database.Models;
using Audiotica.Factory;
using Audiotica.Web.Models;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ConverterModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<TrackToWebSongConverter>().As<IConverter<Track, WebSong>>();
        }
    }
}