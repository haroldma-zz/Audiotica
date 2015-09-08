using System;
using Windows.UI.Xaml.Data;

namespace Audiotica.Windows.Tools.Converters
{
    internal class UniformSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var width = value as double? ?? 0;
            var itemWidth = double.Parse(parameter.ToString());
            if (width <= 0 || double.IsNaN(width)) return itemWidth;

            var columnsNeeded = width/itemWidth;
            columnsNeeded = Math.Truncate(columnsNeeded);
            var uniformWidth = width/ columnsNeeded;
            return uniformWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}