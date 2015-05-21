using System;
using Windows.UI.Xaml.Data;

namespace Audiotica.Converters
{
    public class ContentConverter : IValueConverter
    {
        public object TrueContent { get; set; }
        public object FalseContent { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((bool) value) ? TrueContent : FalseContent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}