using Audiotica.AppEngine;
using Audiotica.AppEngine.Bootstrppers;
using Audiotica.AppEngine.Modules;
using Autofac;

namespace Audiotica.Factories
{
    internal static class AppKernelFactory
    {
        public static Module[] GetModules() =>
            new Module[]
            {
                new UtilityModule(),
                new MatchEngineValidatorModule(),
                new MatchEngineProviderModule(),
                new ServiceModule(),
                new ViewModelModule()
            };

        public static IBootStrapper[] GetBootStrappers() =>
            new IBootStrapper[]
            {
                new CollectionBootStrapper(),
                new MatchEngineBootStrapper()
            };

        public static AppKernel Create() => new AppKernel(GetBootStrappers(), GetModules());
    }
}