using System.Reflection;
using Audiotica.Core.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class MetadataProviderModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
            builder.RegisterType<DesignMetadataProvider>().As<IMetadataProvider>();
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            // Every metadata provider most implement this interface
            var providerInterface = typeof (IMetadataProvider);

            // they should also be located in that assembly (Audiotica.Web)
            var assembly = providerInterface.GetTypeInfo().Assembly;

            var types = assembly.ExportedTypes
                .GetImplementations(providerInterface, excludeTypes: typeof (DesignMetadataProvider));
            foreach (var type in types)
            {
                builder.RegisterType(type).As(providerInterface);
            }
        }
    }
}