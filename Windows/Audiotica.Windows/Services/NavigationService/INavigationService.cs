using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.Windows.Services.NavigationService
{
    public interface INavigationService
    {
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        Type CurrentPageType { get; }
        object CurrentPageParam { get; }
        void NavigatedTo(NavigationMode mode, string parameter);
        bool Navigate(Type page, object parameter = null);
        void RestoreSavedNavigation();
        void GoBack();
        void GoForward();
        void ClearHistory();
        void Suspending();
        void Show(SettingsFlyout flyout, string parameter = null);
    }
}