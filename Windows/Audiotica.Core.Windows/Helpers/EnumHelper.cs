using System;

namespace Audiotica.Core.Windows.Helpers
{
    /// <summary>
    /// Simple helper for converting a string value to
    /// its corresponding Enum literal.
    /// 
    /// e.g. "Running" -> BackgroundTaskState.Running
    /// </summary>
    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}