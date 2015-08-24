using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    public class EmptyListToVisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool any;

            if (value is int)
                any = ((int) value) > 0;
            else if (!(value is IEnumerable<object>))
                return Reverse ? Visibility.Collapsed : Visibility.Visible;
            else
                any = ((IEnumerable<object>) value).Any();

            var result = Reverse && any || !Reverse && !any;

            if (parameter as string == "float")
                return result ? 1 : 0.2;

            return result
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}