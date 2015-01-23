using Windows.System.Display;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica
{
    public static class ScreenTimeoutHelper
    {
        private static readonly DisplayRequest TimeoutDisplayRequest = new DisplayRequest();
        private static bool _currentlyPreventing;

        public static PreventScreenTimeoutOption TimeoutOption
        {
            get
            {
                return (PreventScreenTimeoutOption) App.Locator.AppSettingsHelper.Read<int>("ScreenTimeout",
                    SettingsStrategy.Roaming);
            }
            set { App.Locator.AppSettingsHelper.Write("ScreenTimeout", (int) value, SettingsStrategy.Roaming); }
        }

        public static void OnNowPlayingOpened()
        {
            if (TimeoutOption == PreventScreenTimeoutOption.NowPlayingOnly)
                PreventTimeout();
        }

        public static void OnNowPlayingClosed()
        {
            if (TimeoutOption == PreventScreenTimeoutOption.NowPlayingOnly)
                AllowTimeout();
        }

        public static void OnLaunched()
        {
            if (TimeoutOption == PreventScreenTimeoutOption.Always)
                PreventTimeout();
        }

        public static void PreventTimeout()
        {
            if (_currentlyPreventing) return;

            TimeoutDisplayRequest.RequestActive();
            _currentlyPreventing = true;
        }

        public static void AllowTimeout()
        {
            if (!_currentlyPreventing) return;

            TimeoutDisplayRequest.RequestRelease();
            _currentlyPreventing = false;
        }
    }

    public enum PreventScreenTimeoutOption
    {
        Never,
        Always,
        NowPlayingOnly
    }
}