using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt.Utilities;
using GalaSoft.MvvmLight.Threading;
using Lumia.Imaging;
using Lumia.Imaging.Adjustments;

namespace Audiotica
{
    /// <summary>
    ///     Creates a blur image and assings it to the Image/ImageBrush control.
    /// </summary>
    public class BlurImageTool
    {
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
            
            HttpResponseMessage resp = null;
            Stream stream;

            if (url.StartsWith("http"))
            {
                // Download the image
                using (var client = new HttpClient())
                {
                    resp = await client.GetAsync(url).ConfigureAwait(false);
                    // If it fails, then abort!
                    if (!resp.IsSuccessStatusCode) return;
                    stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                }
            }
            else
            {
                // Get the file
                StorageFile file;

                if (url.StartsWith("ms-appx:"))
                {
                    url = url.Replace("ms-appx:", "");
                }
                if (url.StartsWith("ms-appdata:"))
                {
                    url = url.Replace("ms-appdata:/local/", "");
                    url = url.Replace("ms-appdata:///local/", "");
                    file = await WinRtStorageHelper.GetFileAsync(url).ConfigureAwait(false);
                }
                else if (url.StartsWith("/"))
                    file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx://" + url)).AsTask().ConfigureAwait(false);
                else
                    file = await StorageFile.GetFileFromPathAsync(url).AsTask().ConfigureAwait(false);

                stream = await file.OpenStreamForReadAsync().ConfigureAwait(false);
            }

            using (stream)
            {
                if (stream.Length == 0) return;

                using (var rnd = stream.AsRandomAccessStream())
                {
                    // Then we can create the Random Access Stream Image
                    using (var source = new RandomAccessStreamImageSource(rnd, ImageFormat.Undefined))
                    {
                        // Create effect collection with the source stream
                        using (var filters = new FilterEffect(source))
                        {
                            // Initialize the filter and add the filter to the FilterEffect collection
                            filters.Filters = new IFilter[] {new BlurFilter(50)};

                            // Create a target where the filtered image will be rendered to
                            WriteableBitmap target = null;

                            // Now that you have the raw bytes, create a Image Decoder
                            var decoder = await BitmapDecoder.CreateAsync(rnd).AsTask().ConfigureAwait(false);

                            // Get the first frame from the decoder because we are picking an image
                            var frame = await decoder.GetFrameAsync(0).AsTask().ConfigureAwait(false);

                            // Need to switch to UI thread for this
                            await DispatcherHelper.RunAsync(
                                () =>
                                {
                                    var wid = (int)frame.PixelWidth;
                                    var hgt = (int)frame.PixelHeight;

                                    target = new WriteableBitmap(wid, hgt);
                                }).AsTask().ConfigureAwait(false);

                            // Create a new renderer which outputs WriteableBitmaps
                            using (var renderer = new WriteableBitmapRenderer(filters, target))
                            {
                                rnd.Seek(0);
                                // Render the image with the filter(s)
                                await renderer.RenderAsync().AsTask().ConfigureAwait(false);

                                // Set the output image to Image control as a source
                                // Need to switch to UI thread for this
                                await DispatcherHelper.RunAsync(() =>
                                {
                                    if (image != null) image.Source = target;
                                    else if (imageBrush != null) imageBrush.ImageSource = target;
                                }).AsTask().ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            if (resp != null) resp.Dispose();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof (string),
            typeof (BlurImageTool),
            new PropertyMetadata(string.Empty, SourceCallback));
    }
}