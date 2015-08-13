using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Audiotica.Database.Models;

namespace Audiotica.Windows.Tools.Converters
{
    public class TrackStatusToBoolConverter : IValueConverter
    {

        public Track.TrackStatus DesiredStatus { get; set; }
        public bool NotDesired { get; set; }

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (!(value is Track.TrackStatus))
                return Visibility.Collapsed;

            var status = (Track.TrackStatus)value;
            return (NotDesired && status != DesiredStatus) || (!NotDesired && status == DesiredStatus);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}