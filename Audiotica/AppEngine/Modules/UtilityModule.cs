using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Core.Universal.Utilities;
using Audiotica.Core.Utilities;
using Autofac;

namespace Audiotica.AppEngine.Modules
{
    class UtilityModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
        }

        public override void LoadRunTime(ContainerBuilder builder)
        {
            builder.RegisterType<DispatcherUtility>().As<IDispatcherUtility>();
            builder.RegisterType<CredentialUtility>().As<ICredentialUtility>();
            builder.RegisterType<SettingsUtility>().As<ISettingsUtility>();
            builder.RegisterType<StorageUtility>().As<IStorageUtility>();
        }
    }
}