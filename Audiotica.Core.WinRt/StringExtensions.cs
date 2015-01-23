using Windows.ApplicationModel.Resources;

namespace Audiotica.Core.WinRt
{
    public static class StringExtensions
    {
        public static string FromLanguageResource(this string str)
        {
            var loader = new ResourceLoader();
            return loader.GetString(str);
        }
    }
}