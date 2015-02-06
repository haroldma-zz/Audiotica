using System;

using Windows.UI.Xaml.Data;

namespace Audiotica
{
    public class BoolOpacityConverter : IValueConverter
    {
        public bool IsReverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (bool)value;

            if (state)
            {
                return IsReverse ? 1 : 0.3;
            }
            return IsReverse ? 0.3 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}