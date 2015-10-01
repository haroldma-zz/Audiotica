using System;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var i = (int) value;
            return i == 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var i = (bool) value;
            return i ? 2 : 1;
        }
    }
}