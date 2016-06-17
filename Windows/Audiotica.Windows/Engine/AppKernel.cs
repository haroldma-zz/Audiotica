using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Windows.Engine.Bootstrppers;
using Autofac;

namespace Audiotica.Windows.Engine
{
    public class AppKernel
    {
        private readonly IEnumerable<IBootStrapper> _bootStrappers;

        public AppKernel(IEnumerable<Module> modules, IEnumerable<IBootStrapper> bootStrappers)
        {
            _bootStrappers = bootStrappers;
            var builder = new ContainerBuilder();
            Load(builder, modules);
            Container = builder.Build();
        }

        public IContainer Container { get; set; }

        public ILifetimeScope BeginScope()
        {
            return Container.BeginLifetimeScope();
        }

        public void Load(ContainerBuilder builder, IEnumerable<Module> modules)
        {
            builder.Register(context => BootStrapper.Current?.RootFrame ?? new Frame()).As<Frame>();

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

        public void OnLaunched()
        {
            foreach (var bootstrapper in _bootStrappers)
            {
                using (var scope = BeginScope())

                    try
                    {
                        bootstrapper.OnLaunched(scope);
                    }
                    catch
                    {
                        // ignored
                    }
            }
        }

        public void OnRelaunched()
        {
            var settings = Resolve<ISettingsUtility>();
            foreach (var bootstrapper in _bootStrappers)
            {
                var name = bootstrapper.GetType().Name;
                var key = $"bootstrapper-state-{name}";
                using (var scope = BeginScope())
                    try
                    {
                        bootstrapper.OnRelaunched(scope,
                            settings.Read(key, new Dictionary<string, object>()));
                    }
                    catch
                    {
                        // ignored
                    }

                // Erase the module state
                settings.Remove(key);
            }
        }

        public void OnResuming()
        {
            foreach (var bootstrapper in _bootStrappers)
            {
                using (var scope = BeginScope())
                    try
                    {
                        bootstrapper.OnResuming(scope);
                    }
                    catch
                    {
                        // ignored
                    }
            }
        }

        public void OnSuspending()
        {
            var settings = Resolve<ISettingsUtility>();
            foreach (var bootstrapper in _bootStrappers)
            {
                var name = bootstrapper.GetType().Name;
                var key = $"bootstrapper-state-{name}";
                var dict = new Dictionary<string, object>();

                using (var scope = BeginScope())
                    try
                    {
                        bootstrapper.OnSuspending(scope, dict);
                    }
                    catch
                    {
                        // ignored
                    }

                // Save the module state
                settings.Write(key, dict);
            }
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}