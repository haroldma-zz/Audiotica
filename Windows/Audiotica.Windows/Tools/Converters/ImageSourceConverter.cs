using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Audiotica.Windows.Tools.Converters
{
    internal class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var size = parameter as int?;

            var url = value as string;
            Uri uri;
            if (url == null)
                uri = value as Uri;
            else
                uri = new Uri(url);

            if (uri == null)
                return null;

            var bitmap = new BitmapImage(uri);

            if (size != null)
            {
                bitmap.DecodePixelHeight = size.Value;
                bitmap.DecodePixelWidth = size.Value;
            }

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}