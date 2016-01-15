using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Utilities;
using Audiotica.Windows.Engine.Navigation;

namespace Audiotica.Windows.Engine
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-WindowWrapper
    public class WindowWrapper
    {
        public static WindowWrapper Default() => ActiveWrappers.FirstOrDefault();

        public readonly static List<WindowWrapper> ActiveWrappers = new List<WindowWrapper>();

        public static WindowWrapper Current() => ActiveWrappers.FirstOrDefault(x => x.Window == Window.Current) ?? Default();

        public static WindowWrapper Current(Window window) => ActiveWrappers.FirstOrDefault(x => x.Window == window);

        public static WindowWrapper Current(INavigationService nav) => ActiveWrappers.FirstOrDefault(x => x.NavigationServices.Contains(nav));

        public DisplayInformation DisplayInformation() => Dispatcher.Run(() => global::Windows.Graphics.Display.DisplayInformation.GetForCurrentView());

        public ApplicationView ApplicationView() => Dispatcher.Run(() => global::Windows.UI.ViewManagement.ApplicationView.GetForCurrentView());

        public UIViewSettings UIViewSettings() => Dispatcher.Run(() => global::Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView());

        internal WindowWrapper(Window window)
        {
            if (ActiveWrappers.Any(x => x.Window == window))
                throw new Exception("Windows already has a wrapper; use Current(window) to fetch.");
            Window = window;
            ActiveWrappers.Add(this);
            window.Closed += (s, e) => { ActiveWrappers.Remove(this); };
        }

        public static void ClearNavigationServices(Window window)
        {
            var wrapperToRemove = ActiveWrappers.FirstOrDefault(wrapper => object.ReferenceEquals(wrapper.Window, window));
            if (wrapperToRemove != null)
            {
                wrapperToRemove.NavigationServices.Clear();
            }
        }

        public void Close() { Window.Close(); }
        public Window Window { get; }

        public IDispatcherUtility Dispatcher => App.Current.Kernel.Resolve<IDispatcherUtility>();
        public NavigationServiceList NavigationServices { get; } = new NavigationServiceList();
    }
}
