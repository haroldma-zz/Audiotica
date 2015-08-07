using Audiotica.Core.Common;
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
            builder.RegisterType<WebToTrackSongConverter>().As<IConverter<Track, WebSong>>();
        }
    }
}