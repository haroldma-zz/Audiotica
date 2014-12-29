#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Audiotica.Core.Common
{
    public class NotEmptyToStringConverter : IValueConverter
    {
        public string NotEmptyText { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return string.IsNullOrEmpty(value.ToString()) ? "" : NotEmptyText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}