using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.Services.NavigationService
{
    public class NavigationService : INavigationService
    {
        private const string EmptyNavigation = "1,0";
        private const string SettingsPrefix = "NavService_";
        private const string SettingsNavigationState = SettingsPrefix + "NavigationState";
        private const string SettingsSessions = SettingsPrefix + "NavigationSessions";
        private readonly NavigationFacade _frame;
        private readonly ISettingsUtility _settingsUtility;

        private Dictionary<string, Dictionary<string, object>> _sessions =
            new Dictionary<string, Dictionary<string, object>>();

        public NavigationService(Frame frame, ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
            _frame = new NavigationFacade(frame);
            _frame.Navigating += (s, e) => NavigatedFrom(false);
        }

        private string LastNavigationParameter { get; set; /* TODO: persist */ }
        private string LastNavigationType { get; set; /* TODO: persist */ }
        public bool CanGoBack => _frame.CanGoBack;
        public bool CanGoForward => _frame.CanGoForward;
        public Type CurrentPageType => _frame.CurrentPageType;
        public object CurrentPageParam => _frame.CurrentPageParam;

        public void NavigatedTo(NavigationMode mode, string parameter)
        {
            if (_frame.CurrentPageType == typeof (WelcomePage) && _frame.BackStack.Count > 0)
                _frame.BackStack.RemoveAt(_frame.BackStack.Count - 1);

            _frame.CurrentPageType = _frame.Content.GetType();

            if (_frame.CurrentPageType == typeof (WelcomePage) && _frame.BackStack.Count > 0)
            {
                for (var i = 0; i < _frame.BackStack.Count; i++)
                {
                    _frame.BackStack.RemoveAt(0);
                }
            }

            LastNavigationParameter = parameter;
            LastNavigationType = _frame.Content.GetType().FullName;
            var key = LastNavigationType + "-depth-" + _frame.BackStackDepth;

            if (mode == NavigationMode.New)
            {
                if (_sessions.ContainsKey(key))
                    _sessions[key] = new Dictionary<string, object>();
                else
                    _sessions.Add(key, new Dictionary<string, object>());
            }

            var page = _frame.Content as FrameworkElement;
            var dataContext = page?.DataContext as INavigatable;

            // By using WithTypeInfo, we don't need to know the type of the object for deserializing.
            dataContext?.OnNavigatedTo(parameter.TryDeserializeJsonWithTypeInfo(), mode, _sessions[key]);
        }

        public bool Navigate(Type page, object parameter = null)
        {
            // Seriailizing, if we use non-primitive objects we can still save the nav state.
            // The OnNavigatedTo auto-deserialized, so the ViewModel looks the same as using any object.
            var paramString = parameter.SerializeToJsonWithTypeInfo();

            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (page.FullName.Equals(LastNavigationType)
                && paramString == LastNavigationParameter)
                return false;
            return _frame.Navigate(page, paramString);
        }

        public void RestoreSavedNavigation()
        {
            var state = _settingsUtility.Read(SettingsNavigationState, string.Empty);
            _sessions = _settingsUtility.Read<Dictionary<string, Dictionary<string, object>>>(SettingsSessions, null) ??
                        new Dictionary<string, Dictionary<string, object>>();

            if (string.IsNullOrEmpty(state))
                Navigate(typeof (MainPage));
            else
            {
                _frame.SetNavigationState(state);
                _settingsUtility.Remove(SettingsNavigationState);
                _settingsUtility.Remove(SettingsSessions);
            }
        }

        public void GoBack()
        {
            if (_frame.CanGoBack) _frame.GoBack();
        }

        public void GoForward()
        {
            _frame.GoForward();
        }

        public void ClearHistory()
        {
            _frame.SetNavigationState(EmptyNavigation);
        }

        public void Suspending()
        {
            NavigatedFrom(true);

            var state = _frame.GetNavigationState();
            _settingsUtility.Write(SettingsNavigationState, state);
            _settingsUtility.Write(SettingsSessions, _sessions);
        }

        public void Show(SettingsFlyout flyout, string parameter = null)
        {
            if (flyout == null)
                throw new ArgumentNullException(nameof(flyout));
            var dataContext = flyout.DataContext as INavigatable;
            dataContext?.OnNavigatedTo(parameter, NavigationMode.New, null);
            flyout.Show();
        }

        private void NavigatedFrom(bool suspending)
        {
            var page = _frame.Content as FrameworkElement;
            if (page == null) return;
            var key = page.GetType().FullName + "-depth-" + _frame.BackStackDepth;
            var dataContext = page.DataContext as INavigatable;
            dataContext?.OnNavigatedFrom(suspending, _sessions[key]);
        }
    }

    public class NavigatablePage : Page
    {
        protected override void OnNavigatedTo(global::Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (!DesignMode.DesignModeEnabled)
                App.Current.NavigationService.NavigatedTo(e.NavigationMode, e.Parameter?.ToString());
        }
    }
}