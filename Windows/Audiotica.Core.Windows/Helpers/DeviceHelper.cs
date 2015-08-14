using Windows.System.Profile;

namespace Audiotica.Core.Windows.Helpers
{
    public static class DeviceHelper
    {
        public static bool IsType(DeviceFamily family)
        {
            return $"Windows.{family}" == AnalyticsInfo.VersionInfo.DeviceFamily;
        }
    }

    /// <summary>
    ///     Device Families
    /// </summary>
    public enum DeviceFamily
    {
        /// <summary>
        ///     Unknown
        /// </summary>
        Unknown,

        /// <summary>
        ///     Desktop
        /// </summary>
        Desktop,

        /// <summary>
        ///     Mobile
        /// </summary>
        Mobile,

        /// <summary>
        ///     Team
        /// </summary>
        Team,

        /// <summary>
        ///     Windows IoT
        /// </summary>
        IoT,

        /// <summary>
        ///     Xbox
        /// </summary>
        Xbox
    }
}