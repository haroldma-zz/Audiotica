using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    public class ColorThief
    {
        private const int DefaultColorCount = 5;
        private const int DefaultQuality = 10;
        private const bool DefaultIgnoreWhite = true;

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors and return the base color from the largest cluster.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="quality">
        ///     0 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        public static MMCQ.Swatch GetColor(
            WriteableBitmap sourceImage,
            int quality = DefaultQuality,
            bool ignoreWhite = DefaultIgnoreWhite)
        {
            var palette = GetPalette(sourceImage, DefaultColorCount, quality, ignoreWhite);
            var dominantColor = palette?[0];
            return dominantColor;
        }
        

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <param name="quality">
        ///     0 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        /// <code>true</code>
        public static List<MMCQ.Swatch> GetPalette(
            WriteableBitmap sourceImage,
            int colorCount = DefaultColorCount,
            int quality = DefaultQuality,
            bool ignoreWhite = DefaultIgnoreWhite)
        {
            var cmap = GetColorMap(sourceImage, colorCount, quality, ignoreWhite);
            return cmap?.GeneratePalette();
        }

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        public static MMCQ.CMap GetColorMap(WriteableBitmap sourceImage, int colorCount)
        {
            return GetColorMap(
                sourceImage,
                colorCount,
                DefaultQuality,
                DefaultIgnoreWhite);
        }

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <param name="quality">
        ///     0 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        public static MMCQ.CMap GetColorMap(
            WriteableBitmap sourceImage,
            int colorCount,
            int quality,
            bool ignoreWhite)
        {
            var pixelArray = GetPixelsFast(sourceImage, quality, ignoreWhite);

            // Send array to quantize function which clusters values using median
            // cut algorithm
            var cmap = MMCQ.Quantize(pixelArray, colorCount);
            return cmap;
        }

        private static int[][] GetPixelsFast(
            WriteableBitmap sourceImage,
            int quality,
            bool ignoreWhite)
        {
            var imageData = sourceImage.PixelBuffer;
            var pixels = imageData.ToArray();
            var pixelCount = sourceImage.PixelWidth*sourceImage.PixelHeight;

            var colorDepth = 4;

            var expectedDataLength = pixelCount*colorDepth;
            if (expectedDataLength != pixels.Length)
            {
                throw new ArgumentException("(expectedDataLength = "
                                            + expectedDataLength + ") != (pixels.length = "
                                            + pixels.Length + ")");
            }

            // Store the RGB values in an array format suitable for quantize
            // function

            // numRegardedPixels must be rounded up to avoid an
            // ArrayIndexOutOfBoundsException if all pixels are good.
            var numRegardedPixels = (pixelCount + quality - 1)/quality;

            var numUsedPixels = 0;
            var pixelArray = new int[numRegardedPixels][];


            for (var i = 0; i < pixelCount; i += quality)
            {
                var offset = i*4;
                int b = pixels[offset];
                int g = pixels[offset + 1];
                int r = pixels[offset + 2];
                int a = pixels[offset + 3];

                // If pixel is mostly opaque and not white
                if (a >= 125 && !(ignoreWhite && r > 250 && g > 250 && b > 250))
                {
                    pixelArray[numUsedPixels] = new[] {r, g, b};
                    numUsedPixels++;
                }
            }

            // Remove unused pixels from the array
            var copy = new int[numUsedPixels][];
            Array.Copy(pixelArray, copy, numUsedPixels);
            return copy;
        }

    }
}