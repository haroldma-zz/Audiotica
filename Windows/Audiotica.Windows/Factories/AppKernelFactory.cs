using Audiotica.Windows.AppEngine;
using Audiotica.Windows.AppEngine.Bootstrppers;
using Audiotica.Windows.AppEngine.Modules;
using Autofac;

namespace Audiotica.Windows.Factories
{
    internal static class AppKernelFactory
    {
        public static Module[] GetModules() =>
            new Module[]
            {
                new UtilityModule(),
                new MatchEngineValidatorModule(),
                new MatchEngineProviderModule(),
                new MetadataProviderModule(),
                new ServiceModule(),
                new ViewModelModule()
            };

        public static IBootStrapper[] GetBootStrappers() =>
            new IBootStrapper[]
            {
                // None, atm
            };

        public static AppKernel Create() => new AppKernel(GetBootStrappers(), GetModules());
    }
}