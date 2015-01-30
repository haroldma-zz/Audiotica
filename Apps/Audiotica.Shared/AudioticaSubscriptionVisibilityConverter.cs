using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Audiotica.Data.Model.AudioticaCloud;

namespace Audiotica
{
    public class AudioticaSubscriptionVisibilityConverter
      : IValueConverter
    {
        public SubscriptionType SubscriptionType { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return ((SubscriptionType)value) == SubscriptionType ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    } 
}
