using Audiotica.Core.Utilities.DesignTime;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Utilities.RunTime;
using Audiotica.Core.Windows.Utilities;
using Autofac;

namespace Audiotica.Windows.AppEngine.Modules
{
    internal class UtilityModule : AppModule
    {
        public override void LoadDesignTime(ContainerBuilder builder)
        {
            builder.RegisterType<DesignDispatcherUtility>().As<IDispatcherUtility>();
            builder.RegisterType<DesignCredentialUtility>().As<ICredentialUtility>();
            builder.RegisterType<DesignSettingsUtility>().As<ISettingsUtility>();
            builder.RegisterType<DesignStorageUtility>().As<IStorageUtility>();
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