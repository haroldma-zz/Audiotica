using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.MatchEngine.Providers;
using Autofac;

namespace Audiotica.AppEngine.Modules
{
    internal class MatchEngineProviderModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<MeileProvider>().As<IProvider>();
            builder.RegisterType<NeteaseProvider>().As<IProvider>();
            builder.RegisterType<SoundCloudProvider>().As<IProvider>();
            builder.RegisterType<PleerProvider>().As<IProvider>();
            builder.RegisterType<SongilyProvider>().As<IProvider>();
            builder.RegisterType<Mp3ClanProvider>().As<IProvider>();
            builder.RegisterType<Mp3TruckProvider>().As<IProvider>();
        }
    }
}