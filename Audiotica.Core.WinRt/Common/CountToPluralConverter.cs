#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Audiotica.Core.Common
{
    public class CountToPluralConverter : IValueConverter
    {
        public string SingularText { get; set; }
        public string PluralText { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (int) value > 1 ? PluralText : SingularText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}