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
        public SubscriptionStatus SubscriptionStatus { get; set; }

        public bool IsOpposite { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is SubscriptionType)
            {
                var type = ((SubscriptionType) value);

                if (!IsOpposite)
                return type == SubscriptionType
                           ? Visibility.Visible
                           : Visibility.Collapsed;
                
                return type != SubscriptionType
                           ? Visibility.Visible
                           : Visibility.Collapsed;
            }

            var status = ((SubscriptionStatus) value);
            if (!IsOpposite)
                return status == SubscriptionStatus ? Visibility.Visible : Visibility.Collapsed;
            return status != SubscriptionStatus ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    } 
}
