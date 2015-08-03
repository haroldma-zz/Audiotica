using Audiotica.Core.Windows.Services;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.MatchEngine.Services;
using Audiotica.Windows.Services.NavigationService;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class ServiceModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
            builder.RegisterType<DesignNavigationService>().As<INavigationService>();
            builder.RegisterType<DesignMatchEngineService>().As<IMatchEngineService>();
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<BackgroundAudioService>().As<IBackgroundAudioService>().SingleInstance();
            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<MatchEngineService>().As<IMatchEngineService>();
        }
    }
}