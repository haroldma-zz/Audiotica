using System;
using Windows.UI.Xaml.Data;
using Audiotica.Core.Windows.Helpers;

namespace Audiotica.Windows.Tools.Converters
{
    public class ThemeIntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var i = (int) value;
            return i == 2 || (DeviceHelper.IsType(DeviceFamily.Mobile) && i == 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var i = (bool) value;
            return i ? 2 : 1;
        }
    }
}