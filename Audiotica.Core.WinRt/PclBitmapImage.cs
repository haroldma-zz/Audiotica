#region

using System;
using System.IO;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Common;

#endregion

namespace Audiotica.Core.WinRt
{
    public class PclBitmapImage : IBitmapImage
    {
        public PclBitmapImage(Uri uri)
        {
            Image = new BitmapImage(uri);
        }

        public object Image { get; private set; }

        public void SetUri(Uri uri)
        {
            ((BitmapImage) Image).UriSource = uri;
        }

        public void SetStream(Stream stream)
        {
            ((BitmapImage)Image).SetSource(stream.AsRandomAccessStream());
        }

        public void SetDecodedPixel(int size)
        {
            ((BitmapImage)Image).DecodePixelWidth = size;
        }
    }
}