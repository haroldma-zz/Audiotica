using System;
using System.Collections.Generic;

namespace Audiotica.Windows.Common
{
    public class ColorHistogram
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorHistogram" /> class.
        /// </summary>
        /// <param name="pixels">Pixels array of image contents.</param>
        public ColorHistogram(int[] pixels)
        {
            // Sort the pixels to enable counting below
            Array.Sort(pixels);

            // Count number of distinct colors
            NumberOfColors = CountDistinctColors(pixels);

            // Create arrays
            Colors = new int[NumberOfColors];
            ColorCounts = new int[NumberOfColors];

            // Finally count the frequency of each color
            CountFrequencies(pixels);
        }

        public int[] Colors { get; }
        public int[] ColorCounts { get; }
        public int NumberOfColors { get; }

        private static int CountDistinctColors(IReadOnlyList<int> pixels)
        {
            if (pixels.Count < 2)
            {
                // If we have less than 2 pixels we can stop here
                return pixels.Count;
            }

            // If we have at least 2 pixels, we have a minimum of 1 color...
            var colorCount = 1;
            var currentColor = pixels[0];

            // Now iterate from the second pixel to the end, counting distinct colors
            for (var i = 1; i < pixels.Count; i++)
            {
                // If we encounter a new color, increase the population
                if (pixels[i] != currentColor)
                {
                    currentColor = pixels[i];
                    colorCount++;
                }
            }

            return colorCount;
        }

        private void CountFrequencies(IReadOnlyList<int> pixels)
        {
            if (pixels.Count == 0)
            {
                return;
            }

            var currentColorIndex = 0;
            var currentColor = pixels[0];

            Colors[currentColorIndex] = currentColor;
            ColorCounts[currentColorIndex] = 1;

            if (pixels.Count == 1)
            {
                // If we only have one pixel, we can stop here
                return;
            }

            // Now iterate from the second pixel to the end, population distinct colors
            for (var i = 1; i < pixels.Count; i++)
            {
                if (pixels[i] == currentColor)
                {
                    // We've hit the same color as before, increase population
                    ColorCounts[currentColorIndex]++;
                }
                else
                {
                    // We've hit a new color, increase index
                    currentColor = pixels[i];

                    currentColorIndex++;
                    Colors[currentColorIndex] = currentColor;
                    ColorCounts[currentColorIndex] = 1;
                }
            }
        }
    }
}