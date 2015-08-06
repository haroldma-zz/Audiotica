using Audiotica.Database.Models;
using Audiotica.Factory;
using Audiotica.Web.Models;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ConverterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TrackToWebSongConverter>().As<IConverter<Track, WebSong>>();
        }
    }
}