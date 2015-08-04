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
            var familyText = "Desktop";

            switch (family)
            {
                case Family.Mobile:
                    familyText = "Mobile";
                    break;
                case Family.Xbox:
                    familyText = "Xbox";
                    break;
            }

            return $"Windows.{familyText}" == AnalyticsInfo.VersionInfo.DeviceFamily;
        }
    }
}