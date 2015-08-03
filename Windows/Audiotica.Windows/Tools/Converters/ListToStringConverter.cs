using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var list = value as IEnumerable<string>;
            return list == null ? null : string.Join(", ", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}