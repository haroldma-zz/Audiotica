using Audiotica.Converters;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Web.Models;
using Autofac;

namespace Audiotica.Windows.Engine.Modules
{
    internal class ConverterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WebToTrackConverter>().As<IConverter<WebSong, Track>>();
            builder.RegisterType<WebToArtistConverter>().As<IConverter<WebArtist, Artist>>();
            builder.RegisterType<WebToAlbumConverter>().As<IConverter<WebAlbum, Album>>();
        }
    }
}