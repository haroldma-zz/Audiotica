using Audiotica.Web.MatchEngine.Interfaces;
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
            builder.RegisterType<MeileMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<NeteaseMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<SoundCloudMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<PleerMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<SongilyMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<Mp3ClanMatchProvider>().As<IMatchProvider>();
            builder.RegisterType<Mp3TruckMatchProvider>().As<IMatchProvider>();
        }
    }
}