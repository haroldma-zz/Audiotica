using Audiotica.Services.NavigationService;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.MatchEngine.Services;
using Autofac;

namespace Audiotica.AppEngine.Modules
{
    internal class ServiceModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<NavigationService>();
            builder.RegisterType<MatchEngineService>().As<IMatchEngineService>();
        }
    }
}