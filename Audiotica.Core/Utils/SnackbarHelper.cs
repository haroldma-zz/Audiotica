using System;
namespace Audiotica.Core.Utils
{
    public interface INotificationManager
    {
        void Show(string text, params object[] args);
        void ShowError(string text, params object[] args);
    }
}
