using Audiotica.Core.Utils;
using Audiotica.Core.WinRt.Common;

namespace Audiotica.Core.WinRt.Utilities
{
    public class NotificationManager : INotificationManager
    {
        public void Show(string text)
        {
            Show(text, null);
        }

        public void Show(string text, params object[] args)
        {
            CurtainPrompt.Show(text, args);
        }

        public void ShowError(string text)
        {
            ShowError(text, null);
        }

        public void ShowError(string text, params object[] args)
        {
            CurtainPrompt.ShowError(text, args);
        }
    }
}