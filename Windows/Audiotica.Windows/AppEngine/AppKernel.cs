using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Windows.AppEngine.Bootstrppers;
using Autofac;

namespace Audiotica.Windows.AppEngine
{
    public class AppKernel
    {
        private readonly IEnumerable<IBootStrapper> _bootStrappers;

        public AppKernel(IEnumerable<IBootStrapper> bootStrappers, IEnumerable<Module> modules)
        {
            _bootStrappers = bootStrappers;
            var builder = new ContainerBuilder();
            Load(builder, modules);
            Container = builder.Build();
        }

        public IContainer Container { get; set; }

        public void Load(ContainerBuilder builder, IEnumerable<Module> modules)
        {
            builder.Register(context => App.Current?.RootFrame ?? new Frame()).As<Frame>();

            if (!DesignMode.DesignModeEnabled)
                builder.Register(context => Window.Current.Dispatcher)
                    .As<CoreDispatcher>()
                    .SingleInstance()
                    .AutoActivate();

            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }
        }

        public ILifetimeScope BeginScope()
        {
            return Container.BeginLifetimeScope();
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        public async Task OnLaunchedAsync()
        {
            foreach (var bootstrapper in _bootStrappers)
            {
                using (var scope = BeginScope())
                    await bootstrapper.OnLaunchedAsync(scope);
            }
        }

        public async Task OnRelaunchedAsync()
        {
            var settings = Resolve<ISettingsUtility>();

            foreach (var bootstrapper in _bootStrappers)
            {
                var key = $"module-state-{bootstrapper.GetType().FullName}";
                using (var scope = BeginScope())
                    await bootstrapper.OnRelaunchedAsync(scope, settings.Read(key, new Dictionary<string, object>()));

                // Erase the module state
                settings.Remove(key);
            }
        }

        public async Task OnResumingAsync()
        {
            foreach (var bootstrapper in _bootStrappers)
            {
                using (var scope = BeginScope())
                    await bootstrapper.OnResumingAsync(scope);
            }
        }

        public async Task OnSuspendingAsync()
        {
            var settings = Resolve<ISettingsUtility>();

            foreach (var bootstrapper in _bootStrappers)
            {
                var key = $"module-state-{bootstrapper.GetType().FullName}";
                var dict = new Dictionary<string, object>();

                using (var scope = BeginScope())
                    await bootstrapper.OnSuspendingAsync(scope, dict);

                // Save the module state
                settings.Write(key, dict);
            }
        }
    }
}