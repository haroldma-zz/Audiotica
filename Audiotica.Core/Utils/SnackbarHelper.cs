namespace Audiotica.Core.Utils
{
    public interface INotificationManager
    {
        void Show(string text);
        void Show(string text, params object[] args);
        void ShowError(string text);
        void ShowError(string text, params object[] args);
    }
}
