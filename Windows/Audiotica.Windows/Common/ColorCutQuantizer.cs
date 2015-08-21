using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    /// <summary>
    ///     Class which provides a histogram for RGB values.
    ///     An color quantizer based on the Median-cut algorithm, but optimized for picking out distinct
    ///     colors rather than representation colors.
    ///     The color space is represented as a 3-dimensional cube with each dimension being an RGB
    ///     component.The cube is then repeatedly divided until we have reduced the color space to the
    ///     requested number of colors.An average color is then generated from each cube.
    /// </summary>
    public class ColorCutQuantizer
    {
        private const int ComponentRed = -3;
        private const int ComponentGreen = -2;
        private const int ComponentBlue = -1;
        private readonly Dictionary<int, int> _colorPopulations;
        private readonly int[] _colors;
        private HslColor _tempHsl;

        private ColorCutQuantizer(ColorHistogram colorHistogram, int maxColors)
        {
            var rawColorCount = colorHistogram.NumberOfColors;
            var rawColors = colorHistogram.Colors;
            var rawColorCounts = colorHistogram.ColorCounts;

            // First, lets pack the populations into a SparseIntArray so that they can be easily
            // retrieved without knowing a color's index
            _colorPopulations = new Dictionary<int, int>(rawColorCount);
            for (var i = 0; i < rawColors.Length; i++)
            {
                _colorPopulations.Add(rawColors[i], rawColorCounts[i]);
            }

            // Now go through all of the colors and keep those which we do not want to ignore
            _colors = new int[rawColorCount];
            var validColorCount = 0;
            foreach (var color in rawColors)
            {
                _colors[validColorCount++] = color;
            }

            if (validColorCount <= maxColors)
            {
                // The image has fewer colors than the maximum requested, so just return the colors
                QuantizedColors = new List<Swatch>();
                
                foreach (var color in _colors)
                {
                    QuantizedColors.Add(new Swatch(color.ToColor(), _colorPopulations[color]));
                }
            }
            else
            {
                // We need use quantization to reduce the number of colors
                QuantizedColors = QuantizePixels(validColorCount - 1, maxColors);
            }
        }

        public List<Swatch> QuantizedColors { get; }

        public static ColorCutQuantizer FromBitmap(WriteableBitmap bitmap, int maxColors)
        {
            using (var context = new BitmapContext(bitmap))
                return new ColorCutQuantizer(new ColorHistogram(context.Pixels), maxColors);
        }

        public static ColorCutQuantizer FromPixels(int[] pixels, int maxColors)
        {
            return new ColorCutQuantizer(new ColorHistogram(pixels), maxColors);
        }

        private List<Swatch> QuantizePixels(int maxColorIndex, int maxColors)
        {
            // Create the priority queue which is sorted by volume descending. This means we always
            // split the largest box in the queue
            var pq = new MinHeap<Vbox>(maxColors, new VboxComparer())
            {
                // To start, offer a box which contains all of the colors
                new Vbox(0, maxColorIndex, this)
            };


            // Now go through the boxes, splitting them until we have reached maxColors or there are no
            // more boxes to split
            SplitBoxes(pq, maxColors);

            // Finally, return the average colors of the color boxes
            return GenerateAverageColors(pq);
        }

        /**
         * Iterate through the {@link java.util.Queue}, popping
         * {@link ColorCutQuantizer.Vbox} objects from the queue
         * and splitting them. Once split, the new box and the remaining box are offered back to the
         * queue.
         *
         * @param queue {@link java.util.PriorityQueue} to poll for boxes
         * @param maxSize Maximum amount of boxes to split
         */

        private static void SplitBoxes(Heap<Vbox> queue, int maxSize)
        {
            while (queue.Count < maxSize)
            {
                var vbox = queue.Poll();

                if (vbox != null && vbox.CanSplit())
                {
                    // First split the box, and offer the result
                    queue.Add(vbox.SplitBox());
                    // Then offer the box back
                    queue.Add(vbox);
                }
                else
                {
                    // If we get here then there are no more boxes to split, so return
                    return;
                }
            }
        }

        private static List<Swatch> GenerateAverageColors(Heap<Vbox> vboxes)
        {
            var colors = new List<Swatch>(vboxes.Count);
            colors.AddRange(vboxes.Select(vbox => vbox.GetAverageColor()));
            return colors;
        }

        /**
         * Modify the significant octet in a packed color int. Allows sorting based on the value of a
         * single color component.
         *
         * @see Vbox#findSplitPoint()
         */

        private void ModifySignificantOctet(int dimension, int lowerIndex, int upperIndex)
        {
            switch (dimension)
            {
                case ComponentRed:
                    // Already in RGB, no need to do anything
                    break;
                case ComponentGreen:
                    // We need to do a RGB to GRB swap, or vice-versa
                    for (var i = lowerIndex; i <= upperIndex; i++)
                    {
                        var color = _colors[i];
                        _colors[i] = ColorExtensions.Rgb((color >> 8) & 0xFF, (color >> 16) & 0xFF, color & 0xFF);
                    }
                    break;
                case ComponentBlue:
                    // We need to do a RGB to BGR swap, or vice-versa
                    for (var i = lowerIndex; i <= upperIndex; i++)
                    {
                        var color = _colors[i];
                        _colors[i] = ColorExtensions.Rgb(color & 0xFF, (color >> 8) & 0xFF, (color >> 16) & 0xFF);
                    }
                    break;
            }
        }
        
        /**
         * Represents a tightly fitting box around a color space.
         */

        private class Vbox
        {
            private readonly ColorCutQuantizer _colorCutQuantizer;
            // lower and upper index are inclusive
            private readonly int _lowerIndex;
            private int _minBlue, _maxBlue;
            private int _minGreen, _maxGreen;
            private int _minRed, _maxRed;
            private int _upperIndex;

            public Vbox(int lowerIndex, int upperIndex, ColorCutQuantizer colorCutQuantizer)
            {
                _lowerIndex = lowerIndex;
                _upperIndex = upperIndex;
                _colorCutQuantizer = colorCutQuantizer;
                FitBox();
            }

            public int GetVolume()
            {
                return (_maxRed - _minRed + 1)*(_maxGreen - _minGreen + 1)*
                       (_maxBlue - _minBlue + 1);
            }

            public bool CanSplit()
            {
                return GetColorCount() > 1;
            }

            private int GetColorCount()
            {
                return _upperIndex - _lowerIndex + 1;
            }

            /// <summary>
            ///     Recomputes the boundaries of this box to tightly fit the colors within the box.
            /// </summary>
            private void FitBox()
            {
                // Reset the min and max to opposite values
                _minRed = _minGreen = _minBlue = 0xFF;
                _maxRed = _maxGreen = _maxBlue = 0x0;

                for (var i = _lowerIndex; i <= _upperIndex; i++)
                {
                    var color = _colorCutQuantizer._colors[i];
                    var r = ColorExtensions.Red(color);
                    var g = ColorExtensions.Green(color);
                    var b = ColorExtensions.Blue(color);
                    if (r > _maxRed)
                    {
                        _maxRed = r;
                    }
                    if (r < _minRed)
                    {
                        _minRed = r;
                    }
                    if (g > _maxGreen)
                    {
                        _maxGreen = g;
                    }
                    if (g < _minGreen)
                    {
                        _minGreen = g;
                    }
                    if (b > _maxBlue)
                    {
                        _maxBlue = b;
                    }
                    if (b < _minBlue)
                    {
                        _minBlue = b;
                    }
                }
            }

            /// <summary>
            ///     Split this color box at the mid-point along it's longest dimension.
            /// </summary>
            /// <returns>the new ColorBox</returns>
            /// <exception cref="System.Exception">Can not split a box with only 1 color</exception>
            public Vbox SplitBox()
            {
                if (!CanSplit())
                {
                    throw new Exception("Can not split a box with only 1 color");
                }

                // find median along the longest dimension
                var splitPoint = FindSplitPoint();

                var newBox = new Vbox(splitPoint + 1, _upperIndex, _colorCutQuantizer);

                // Now change this box's upperIndex and recompute the color boundaries
                _upperIndex = splitPoint;
                FitBox();

                return newBox;
            }

            /// <summary>
            ///     Gets the dimension which this box is largest in
            /// </summary>
            /// <returns></returns>
            private int GetLongestColorDimension()
            {
                var redLength = _maxRed - _minRed;
                var greenLength = _maxGreen - _minGreen;
                var blueLength = _maxBlue - _minBlue;

                if (redLength >= greenLength && redLength >= blueLength)
                {
                    return ComponentRed;
                }
                if (greenLength >= redLength && greenLength >= blueLength)
                {
                    return ComponentGreen;
                }
                return ComponentBlue;
            }

            /**
             * Finds the point within this box's lowerIndex and upperIndex index of where to split.
             *
            This is calculated by finding the longest color dimension, and then sorting the
            sub-array based on that dimension value in each color. The colors are then iterated over
            until a color is found with at least the midpoint of the whole box's dimension midpoint.
             *
             * @return the index of the colors array to split from
             */

            /// <summary>
            ///     Finds the point within this box's lowerIndex and upperIndex index of where to split.
            ///     This is calculated by finding the longest color dimension, and then sorting the
            ///     sub-array based on that dimension value in each color.The colors are then iterated over
            ///     until a color is found with at least the midpoint of the whole box's dimension midpoint.
            /// </summary>
            /// <returns>the index of the colors array to split from</returns>
            private int FindSplitPoint()
            {
                var longestDimension = GetLongestColorDimension();

                // We need to sort the colors in this box based on the longest color dimension.
                // As we can't use a Comparator to define the sort logic, we modify each color so that
                // it's most significant is the desired dimension
                _colorCutQuantizer.ModifySignificantOctet(longestDimension, _lowerIndex, _upperIndex);
                
                Array.Sort(_colorCutQuantizer._colors, _lowerIndex, _upperIndex- _lowerIndex);

                // Now revert all of the colors so that they are packed as RGB again
                _colorCutQuantizer.ModifySignificantOctet(longestDimension, _lowerIndex, _upperIndex);

                var dimensionMidPoint = MidPoint(longestDimension);

                for (var i = _lowerIndex; i <= _upperIndex; i++)
                {
                    var color = _colorCutQuantizer._colors[i];

                    switch (longestDimension)
                    {
                        case ComponentRed:
                            if (ColorExtensions.Red(color) >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case ComponentGreen:
                            if (ColorExtensions.Green(color) >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case ComponentBlue:
                            if (ColorExtensions.Blue(color) > dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                    }
                }

                return _lowerIndex;
            }

            /// <summary>
            ///     Gets the average color of this box.
            /// </summary>
            /// <returns></returns>
            public Swatch GetAverageColor()
            {
                var redSum = 0;
                var greenSum = 0;
                var blueSum = 0;
                var totalPopulation = 0;

                for (var i = _lowerIndex; i <= _upperIndex; i++)
                {
                    var color = _colorCutQuantizer._colors[i];
                    var colorPopulation = _colorCutQuantizer._colorPopulations[color];

                    totalPopulation += colorPopulation;
                    redSum += colorPopulation*ColorExtensions.Red(color);
                    greenSum += colorPopulation*ColorExtensions.Green(color);
                    blueSum += colorPopulation*ColorExtensions.Blue(color);
                }

                var redAverage = (int) Math.Round(redSum/(float) totalPopulation);
                var greenAverage = (int) Math.Round(greenSum/(float) totalPopulation);
                var blueAverage = (int) Math.Round(blueSum/(float) totalPopulation);

                return new Swatch(redAverage, greenAverage, blueAverage, totalPopulation);
            }

            /**
             * @return the midpoint of this box in the given {@code dimension}
             */

            /// <summary>
            ///     Gets the midpoint of this box in the given dimension.
            /// </summary>
            /// <param name="dimension">The dimension.</param>
            /// <returns></returns>
            private int MidPoint(int dimension)
            {
                switch (dimension)
                {
                    default:
                        return (_minRed + _maxRed)/2;
                    case ComponentGreen:
                        return (_minGreen + _maxGreen)/2;
                    case ComponentBlue:
                        return (_minBlue + _maxBlue)/2;
                }
            }
        }

        private class VboxComparer : Comparer<Vbox>
        {
            public override int Compare(Vbox x, Vbox y)
            {
                return x.GetVolume() - y.GetVolume();
            }
        }
    }
}