using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Audiotica.Windows.Tools.Converters
{
    public class BoolToColorBrushConverter : IValueConverter
    {
        public SolidColorBrush TrueBrush { get; set; } = new SolidColorBrush(Colors.Gold);
        public SolidColorBrush FalseBrush { get; set; } = new SolidColorBrush(Colors.Black);

        public object Convert(object value, Type targetType, object parameter, string language) => value is bool && ((bool) value) ? TrueBrush : FalseBrush;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}