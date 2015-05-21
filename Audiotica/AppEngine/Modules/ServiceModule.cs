using Audiotica.Services.NavigationService;
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
        }
    }
}