using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Audiotica
{
    public static class WaveFormDrawer
    {
        public static async Task<BitmapImage> DrawNormalizedAudio(List<float> data, Color foreColor, Color backColor, Size imageSize)
        {
            var bmp = new WriteableBitmap((int)imageSize.Width, (int)imageSize.Height);

            const int borderWidth = 0;
            float width = bmp.PixelWidth - (2 * borderWidth);
            float height = bmp.PixelHeight - (2 * borderWidth);


            bmp.Clear(backColor);

            float size = data.Count;


            for (var iPixel = 0; iPixel < width; iPixel += 1)
            {
                int yMax = 0;
                int yMin = 0;
                await Task.Run(() =>
                {
                    // determine start and end points within WAV
                    var start = (int) (iPixel*(size/width));
                    var end = (int) ((iPixel + 1)*(size/width));
                    if (end > data.Count)
                        end = data.Count;

                    float posAvg, negAvg;
                    Averages(data, start, end, out posAvg, out negAvg);

                    yMax = (int) (borderWidth + height - ((posAvg + 1)*.5f*height));
                    yMin = (int) (borderWidth + height - ((negAvg + 1)*.5f*height));
                });

                bmp.DrawLine(iPixel + borderWidth, yMax, iPixel + borderWidth, yMin, foreColor);
            }

            var rnd = new InMemoryRandomAccessStream();
            await bmp.ToStream(rnd, BitmapEncoder.PngEncoderId);

            var artwork = new BitmapImage();
            await artwork.SetSourceAsync(rnd);

            return artwork;
        }


        private static void Averages(List<float> data, int startIndex, int endIndex, out float posAvg, out float negAvg)
        {
            posAvg = 0.0f;
            negAvg = 0.0f;

            int posCount = 0, negCount = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (float.IsNaN(data[i]) 
                    || float.IsPositiveInfinity(data[i]) 
                    || float.IsNegativeInfinity(data[i])
                    || data[i] < -50
                    || data[i] > 50)
                    continue;

                if (data[i] > 0)
                {
                    posCount++;
                    posAvg += data[i];
                }
                else
                {
                    negCount++;
                    negAvg += data[i];
                }
            }

            posAvg /= posCount;
            negAvg /= negCount;
        }

        public static float[] FloatArrayFromByteArray(byte[] input)
        {
            var output = new float[input.Length / 4];
            for (var i = 0; i < output.Length; i++)
            {
                output[i] = BitConverter.ToSingle(input, i * 4);
            }
            return output;
        }
    }
}
