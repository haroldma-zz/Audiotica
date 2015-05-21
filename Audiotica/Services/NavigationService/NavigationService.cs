using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Core.Utilities;
using Audiotica.Views;

namespace Audiotica.Services.NavigationService
{
    public class NavigationService
    {
        private const string EmptyNavigation = "1,0";
        private const string SettingsPrefix = "NavService_";
        private const string SettingsNavigationState = SettingsPrefix + "NavigationState";
        private const string SettingsSessions = SettingsPrefix + "NavigationSessions";
        private readonly NavigationFacade _frame;
        private readonly ISettingsUtility _settingsUtility;
        private Dictionary<string, Dictionary<string, object>> _sessions = new Dictionary<string, Dictionary<string, object>>();

        public NavigationService(Frame frame, ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
            _frame = new NavigationFacade(frame);
            _frame.Navigating += (s, e) => NavigatedFrom(false);
        }

        public bool CanGoBack => _frame.CanGoBack;
        public bool CanGoForward => _frame.CanGoForward;
        public Type CurrentPageType => _frame.CurrentPageType;
        public object CurrentPageParam => _frame.CurrentPageParam;
        private object LastNavigationParameter { get; set; /* TODO: persist */ }
        private string LastNavigationType { get; set; /* TODO: persist */ }

        private void NavigatedFrom(bool suspending)
        {
            var page = _frame.Content as FrameworkElement;
            if (page == null) return;
            var key = page.GetType().FullName + "-depth-" + _frame.BackStackDepth;
            var dataContext = page.DataContext as INavigatable;
            dataContext?.OnNavigatedFrom(suspending, _sessions[key]);
        }

        public void NavigatedTo(NavigationMode mode, object parameter)
        {
            _frame.CurrentPageType = _frame.Content.GetType();
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
            
            dataContext?.OnNavigatedTo(parameter, mode, _sessions[key]);
        }

        public bool Navigate(Type page, object parameter = null)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (page.FullName.Equals(LastNavigationType)
                && parameter == LastNavigationParameter)
                return false;
            return _frame.Navigate(page, parameter);
        }

        public void RestoreSavedNavigation()
        {
            var state = _settingsUtility.Read(SettingsNavigationState);
            _sessions = _settingsUtility.ReadJsonAs<Dictionary<string, Dictionary<string, object>>>(SettingsSessions) ?? new Dictionary<string, Dictionary<string, object>>();

            if (string.IsNullOrEmpty(state))
                Navigate(typeof (MainPage));
            else
            {
                _frame.SetNavigationState(state);
                _settingsUtility.Write(SettingsNavigationState, null);
                _settingsUtility.Write(SettingsSessions, null);
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
            _settingsUtility.WriteAsJson(SettingsSessions, _sessions); 
        }

        public void Show(SettingsFlyout flyout, string parameter = null)
        {
            if (flyout == null)
                throw new ArgumentNullException(nameof(flyout));
            var dataContext = flyout.DataContext as INavigatable;
            dataContext?.OnNavigatedTo(parameter, NavigationMode.New, null);
            flyout.Show();
        }
    }

    public class NavigatablePage : Page
    {
        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            App.Current.NavigationService.NavigatedTo(e.NavigationMode, e.Parameter);
        }
    }
}