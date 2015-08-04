using System.Reflection;
using Audiotica.Core.Extensions;
using Audiotica.Web.MatchEngine.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class MatchEngineProviderModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            // Every provider most implement this interface
            var providerInterface = typeof (IMatchProvider);

            // they should also be located in that assembly (Audiotica.Web)
            var assembly = providerInterface.GetTypeInfo().Assembly;

            var types = assembly.ExportedTypes.GetImplementations(providerInterface);
            foreach (var type in types)
            {
                builder.RegisterType(type).As(providerInterface);
            }
        }
    }
}