using System;
using Windows.UI.ViewManagement;

namespace Audiotica
{
    public static class StatusBarHelper
    {
        public static StatusBar StatusBar
        {
            get { return StatusBar.GetForCurrentView(); }
        }

        public static async void ShowStatus(string message)
        {
            StatusBar.ProgressIndicator.Text = message;
            await StatusBar.ProgressIndicator.ShowAsync();
        }

        public static async void HideStatus()
        {
            await StatusBar.ProgressIndicator.HideAsync();
        }
    }
}
