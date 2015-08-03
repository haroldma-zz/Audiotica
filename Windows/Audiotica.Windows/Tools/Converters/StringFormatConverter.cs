using System;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public string Format { get; set; }
        public bool UseToString { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (UseToString)
            {
                if (value is DateTime)
                    return ((DateTime) value).ToString(Format);
                if (value is TimeSpan)
                    return ((TimeSpan) value).ToString(Format);
            }
            return string.Format(Format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}