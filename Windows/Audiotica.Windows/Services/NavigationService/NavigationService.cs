using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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

        private readonly SystemNavigationManager _currentView;

        public NavigationService(Frame frame, ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
            _frame = new NavigationFacade(frame);
            _frame.Navigating += (s, e) => NavigatedFrom(false);

            _currentView = SystemNavigationManager.GetForCurrentView();
        }

        public Type DefaultPage => typeof (ExplorePage);
        public bool CanGoBack => _frame.CanGoBack;
        public bool CanGoForward => _frame.CanGoForward;
        public Type CurrentPageType => _frame.CurrentPageType;
        public string CurrentPageParam => _frame.CurrentPageParam;

        public void NavigatedTo(NavigationMode mode, string parameter)
        {
            _currentView.AppViewBackButtonVisibility = _frame.BackStackDepth > 1 ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;

            _frame.CurrentPageParam = parameter;
            _frame.CurrentPageType = _frame.Content.GetType();
            var key = CurrentPageType + "-depth-" + _frame.BackStackDepth;

            if (mode == NavigationMode.New)
            {
                if (_sessions.ContainsKey(key))
                    _sessions[key] = new Dictionary<string, object>();
                else
                    _sessions.Add(key, new Dictionary<string, object>());
            }

            var page = _frame.Content as FrameworkElement;
            var dataContext = page?.DataContext as INavigatable;

            dataContext?.OnNavigatedTo(parameter, mode, _sessions[key]);
        }

        public bool Navigate(Type page, string parameter = null)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (page == CurrentPageType
                && parameter == CurrentPageParam)
                return false;
            return _frame.Navigate(page, parameter);
        }

        public void RestoreSavedNavigation()
        {
            var state = _settingsUtility.Read(SettingsNavigationState, string.Empty);
            _sessions = _settingsUtility.Read<Dictionary<string, Dictionary<string, object>>>(SettingsSessions, null) ??
                        new Dictionary<string, Dictionary<string, object>>();

            if (string.IsNullOrEmpty(state))
                Navigate(DefaultPage);
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