#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Audiotica.Core.Common
{
    public class EmptyListToVisibilityConverter : IValueConverter
    {
        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var emptyList = value is int ? (int)value == 0 : !(value as IEnumerable<object>).Any();

            if (!Reverse && emptyList)
                return Visibility.Visible;
            if (Reverse && !emptyList)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}