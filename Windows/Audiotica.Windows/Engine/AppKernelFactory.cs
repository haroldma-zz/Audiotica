using Audiotica.Windows.Engine.Bootstrppers;
using Audiotica.Windows.Engine.Modules;
using Autofac;

namespace Audiotica.Windows.Engine
{
    internal static class AppKernelFactory
    {
        public static AppKernel Create() => new AppKernel(GetModules(), GetBootStrappers());

        public static IBootStrapper[] GetBootStrappers() =>
            new IBootStrapper[]
            {
                new BackgroundAudioBootstrapper(),
                new LibraryBootstrapper(),
                new DownloadServiceBootstrapper()
            };

        public static Module[] GetModules() =>
            new Module[]
            {
                new UtilityModule(),
                new MatchEngineValidatorModule(),
                new MatchEngineProviderModule(),
                new MetadataProviderModule(),
                new ConverterModule(),
                new ServiceModule(),
                new ViewModelModule()
            };
    }
}