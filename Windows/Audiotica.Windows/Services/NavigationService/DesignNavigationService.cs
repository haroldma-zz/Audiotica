using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.Windows.Services.NavigationService
{
    public class DesignNavigationService : INavigationService
    {
        public Type DefaultPage { get; }
        public bool CanGoBack { get; }
        public bool CanGoForward { get; }
        public Type CurrentPageType { get; }
        public string CurrentPageParam { get; }

        public void NavigatedTo(NavigationMode mode, string parameter)
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type page, object parameter = null)
        {
            throw new NotImplementedException();
        }

        public void RestoreSavedNavigation()
        {
            throw new NotImplementedException();
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void GoForward()
        {
            throw new NotImplementedException();
        }

        public void ClearHistory()
        {
            throw new NotImplementedException();
        }

        public void Suspending()
        {
            throw new NotImplementedException();
        }

        public void Show(SettingsFlyout flyout, object parameter = null)
        {
            throw new NotImplementedException();
        }
    }
}