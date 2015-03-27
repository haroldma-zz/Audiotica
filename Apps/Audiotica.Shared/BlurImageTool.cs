using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.WinRt.Utilities;
using GalaSoft.MvvmLight.Threading;
using Buffer = System.Buffer;

namespace Audiotica
{
    /// <summary>
    ///     Creates a blur image and assings it to the Image/ImageBrush control.
    /// </summary>
    public class BlurImageTool
    {
        public static void BoxBlur(int[] pixels, uint w, uint h, int range)
        {
            if ((range & 1) == 0)
            {
                throw new InvalidOperationException("Range must be odd!");
            }

            BoxBlurHorizontal(pixels, w, h, range);
            BoxBlurVertical(pixels, w, h, range);
        }

        public static void BoxBlurHorizontal(int[] pixels, uint w, uint h, int range)
        {
            var halfRange = range/2;
            var index = 0;
            var newColors = new int[w];

            for (var y = 0; y < h; y++)
            {
                var hits = 0;
                var r = 0;
                var g = 0;
                var b = 0;
                for (var x = -halfRange; x < w; x++)
                {
                    var oldPixel = x - halfRange - 1;
                    if (oldPixel >= 0)
                    {
                        var col = pixels[index + oldPixel];
                        if (col != 0)
                        {
                            r -= ((byte) (col >> 16));
                            g -= ((byte) (col >> 8));
                            b -= ((byte) col);
                        }
                        hits--;
                    }

                    var newPixel = x + halfRange;
                    if (newPixel < w)
                    {
                        var col = pixels[index + newPixel];
                        if (col != 0)
                        {
                            r += ((byte) (col >> 16));
                            g += ((byte) (col >> 8));
                            b += ((byte) col);
                        }
                        hits++;
                    }

                    if (x >= 0)
                    {
                        var color =
                            (255 << 24)
                            | ((byte) (r/hits) << 16)
                            | ((byte) (g/hits) << 8)
                            | ((byte) (b/hits));

                        newColors[x] = color;
                    }
                }

                for (var x = 0; x < w; x++)
                {
                    pixels[index + x] = newColors[x];
                }

                index += (int)w;
            }
        }

        public static void BoxBlurVertical(int[] pixels, uint w, uint h, int range)
        {
            var halfRange = range/2;

            var newColors = new int[h];
            var oldPixelOffset = -(halfRange + 1)*w;
            var newPixelOffset = (halfRange)*w;

            for (var x = 0; x < w; x++)
            {
                var hits = 0;
                var r = 0;
                var g = 0;
                var b = 0;
                var index = -halfRange*w + x;
                for (var y = -halfRange; y < h; y++)
                {
                    var oldPixel = y - halfRange - 1;
                    if (oldPixel >= 0)
                    {
                        var col = pixels[index + oldPixelOffset];
                        if (col != 0)
                        {
                            r -= ((byte) (col >> 16));
                            g -= ((byte) (col >> 8));
                            b -= ((byte) col);
                        }
                        hits--;
                    }

                    var newPixel = y + halfRange;
                    if (newPixel < h)
                    {
                        var col = pixels[index + newPixelOffset];
                        if (col != 0)
                        {
                            r += ((byte) (col >> 16));
                            g += ((byte) (col >> 8));
                            b += ((byte) col);
                        }
                        hits++;
                    }

                    if (y >= 0)
                    {
                        var color =
                            (255 << 24)
                            | ((byte) (r/hits) << 16)
                            | ((byte) (g/hits) << 8)
                            | ((byte) (b/hits));

                        newColors[y] = color;
                    }

                    index += w;
                }

                for (var y = 0; y < h; y++)
                {
                    pixels[y*w + x] = newColors[y];
                }
            }
        }

        public static string GetSource(DependencyObject element)
        {
            return (string) element.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject element, string value)
        {
            element.SetValue(SourceProperty, value);
        }

        private static async void SourceCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var url = e.NewValue as string;
            var image = d as Image;
            var imageBrush = d as ImageBrush;

            if (image != null) image.Source = null;
            else if (imageBrush != null) imageBrush.ImageSource = null;

            if (string.IsNullOrEmpty(url) || (image == null && imageBrush == null)) return;

            Stream stream;

            if (url.StartsWith("http"))
            {
                // Download the image
                using (var client = new HttpClient())
                {
                    using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                    {
                        // If it fails, then abort!
                        if (!resp.IsSuccessStatusCode) return;

                        var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        stream = new MemoryStream(bytes);
                    }
                }
            }
            else
            {
                // Get the file
                StorageFile file;

                if (url.StartsWith("ms-appx:"))
                {
                    url = url.Replace("ms-appx://", "");
                    url = url.Replace("ms-appx:", "");
                }
                if (url.StartsWith("ms-appdata:"))
                {
                    url = url.Replace("ms-appdata:/local/", "");
                    url = url.Replace("ms-appdata:///local/", "");
                    file = await WinRtStorageHelper.GetFileAsync(url).ConfigureAwait(false);
                }
                else if (url.StartsWith("/"))
                    file =
                        await
                            StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx://" + url))
                                .AsTask()
                                .ConfigureAwait(false);
                else
                    file = await StorageFile.GetFileFromPathAsync(url).AsTask().ConfigureAwait(false);

                stream = await file.OpenStreamForReadAsync().ConfigureAwait(false);
            }

            using (stream)
            {
                if (stream.Length == 0) return;

                    var blurPercent = 13;

                    await DispatcherHelper.RunAsync(
                        () => blurPercent = GetBlurPercent(d));

                    // Now that you have the raw bytes, create a Image Decoder
                    var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);

                    // Get the first frame from the decoder because we are picking an image
                    var frame = await decoder.GetFrameAsync(0).AsTask().ConfigureAwait(false);

                    // Need to switch to UI thread for this
                    await DispatcherHelper.RunAsync(
                        async () =>
                        {
                            var wid = (int) frame.PixelWidth;
                            var hgt = (int) frame.PixelHeight;
                            
                            var target = new WriteableBitmap(wid, hgt);
                            stream.Position = 0;
                            await target.SetSourceAsync(stream.AsRandomAccessStream());

                            var pixelStream = target.PixelBuffer.AsStream();
                            var data = new byte[pixelStream.Length];
                            await pixelStream.ReadAsync(data, 0, data.Length);
                            pixelStream.Position = 0;

                            await Task.Factory.StartNew(
                                () =>
                                {
                                    // Lets get the pixel data, by converty the binary array to int[]
                                    var pixels = new int[data.Length * 4];

                                    // and copy it
                                    Buffer.BlockCopy(data, 0, pixels, 0, data.Length);

                                    // apply the box blur
                                    BoxBlur(pixels, frame.PixelWidth, frame.PixelHeight, blurPercent);

                                    // now copy the int[] back to the byte[]
                                    Buffer.BlockCopy(pixels, 0, data, 0, data.Length);
                                });

                            // so we can write it to the pixel buffer stream
                            await pixelStream.WriteAsync(data, 0, data.Length);

                            if (image != null) image.Source = target;
                            else if (imageBrush != null) imageBrush.ImageSource = target;
                        }).AsTask().ConfigureAwait(false);
                
            }
        }

        public static int GetBlurPercent(DependencyObject element)
        {
            return (int) element.GetValue(BlurPercentProperty);
        }

        public static void SetBlurPercent(DependencyObject element, int value)
        {
            element.SetValue(BlurPercentProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof (string),
            typeof (BlurImageTool),
            new PropertyMetadata(string.Empty, SourceCallback));

        public static readonly DependencyProperty BlurPercentProperty = DependencyProperty.RegisterAttached(
            "BlurPercent",
            typeof (int),
            typeof (BlurImageTool),
            new PropertyMetadata(13, null));
    }
}