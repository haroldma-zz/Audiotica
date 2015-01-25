using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Android.Utilities
{
    public static class SettingsHelper
    {
        public static IAppSettingsHelper AppSettingsHelper
        {
            get { return App.Current.Locator.AppSettingsHelper; }
        }

        public static RepeatMode RepeatMode
        {
            get { return AppSettingsHelper.Read<RepeatMode>("RepeatMode"); }
            set { AppSettingsHelper.Write("RepeatMode", value); }
        }

        public static int CurrentQueueId
        {
            get { return AppSettingsHelper.Read<int>(PlayerConstants.CurrentTrack); }
            set { AppSettingsHelper.Write(PlayerConstants.CurrentTrack, value); }
        }
    }
}