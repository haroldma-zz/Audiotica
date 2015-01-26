using System;
using Android.App;
using Android.Graphics;
using Audiotica.Core.Utils;
using SnackbarSharp;

namespace Audiotica.Android.Implementations
{
    public class NotificationManager : INotificationManager
    {
        public void Show(string text, params object[] args)
        {
            Show(text, Color.Green, args);
        }

        public void ShowError(string text, params object[] args)
        {
            Show(text, Color.Red, args);
        }

        private void Show(string text, Color color, params object[] args)
        {
            if (args != null)
                text = string.Format(text, args);
			App.Current.Locator.DispatcherHelper.RunAsync(() => SnackbarManager.Show(new Snackbar(App.Current.CurrentActivity).Text(text).Color(color)));
        }
    }
}