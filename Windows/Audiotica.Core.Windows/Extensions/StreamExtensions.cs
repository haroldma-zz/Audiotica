using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace Audiotica.Core.Windows.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<WriteableBitmap> ToWriteableBitmapAsync(this Stream stream)
        {
            var randomAccessStream = stream.AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

            // Get the first frame from the decoder
            var frame = await decoder.GetFrameAsync(0);

            var wid = (int) frame.PixelWidth;
            var hgt = (int) frame.PixelHeight;

            var target = new WriteableBitmap(wid, hgt);

            randomAccessStream.Seek(0);
            await target.SetSourceAsync(randomAccessStream);
            return target;
        }
    }
}