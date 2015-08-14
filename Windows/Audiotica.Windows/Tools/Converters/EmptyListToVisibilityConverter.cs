using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class EmptyListToVisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is IEnumerable<object>))
                return Visibility.Collapsed;
            var any = ((IEnumerable<object>) value).Any();

            return Reverse && any || !Reverse && !any
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}