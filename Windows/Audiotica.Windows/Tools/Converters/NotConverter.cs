using System;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool)) return false;
            return !((bool) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}