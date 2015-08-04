using System.Reflection;
using Audiotica.Core.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class MetadataProviderModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            // Every metadata provider most implement this interface
            var providerInterface = typeof (IMetadataProvider);

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