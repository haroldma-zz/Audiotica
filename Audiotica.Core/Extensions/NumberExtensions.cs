using System;

namespace Audiotica.Core.Extensions
{
    public static class NumberExtensions
    {
        public static string ToFormattedBytes(this float byteCount) => ToFormattedBytes((double) byteCount);
        public static string ToFormattedBytes(this long byteCount) => ToFormattedBytes((double) byteCount);
        public static string ToFormattedBytes(this int byteCount) => ToFormattedBytes((double) byteCount);

        public static string ToFormattedBytes(this double byteCount)
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB", "EB"}; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes/Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount)*num) + suf[place];
        }
    }
}