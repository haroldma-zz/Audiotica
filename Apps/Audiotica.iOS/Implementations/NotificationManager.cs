using Audiotica.Core.Utils;

using UIKit;

namespace Audiotica.iOS.Implementations
{
    internal class NotificationManager : INotificationManager
    {
        public void Show(string text, params object[] args)
        {
            this.Show(text, UIColor.Green, args);
        }

        public void ShowError(string text, params object[] args)
        {
            this.Show(text, UIColor.Red, args);
        }

        private void Show(string text, UIColor color, params object[] args)
        {
            if (args != null)
            {
                text = string.Format(text, args);
            }

            // App.Current.Locator.DispatcherHelper.RunAsync(() => SnackbarManager.Show(new Snackbar(App.Current.CurrentActivity).Text(text).Color(color)));
        }
    }
}