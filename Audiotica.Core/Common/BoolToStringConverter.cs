#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Audiotica.Core.Common
{
    public class BoolToStringConverter : IValueConverter
    {
        public string TrueContent { get; set; }
        public string FalseContent { get; set; }

        public string NotEmptyText { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (bool)value ? TrueContent : FalseContent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}