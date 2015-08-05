using Windows.System.Profile;

namespace Audiotica.Core.Windows.Helpers
{
    public static class DeviceHelper
    {
        public enum Family
        {
            Desktop,
            Mobile,
            Xbox
        }

        public static bool IsType(Family family)
        {
            return $"Windows.{family}" == AnalyticsInfo.VersionInfo.DeviceFamily;
        }
    }
}